using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    class PlantUmlNamedPipeServer : IDisposable {

        private const string NamedPipeServerNamePrefix = "PlantUmlNPServer";

        NamedPipeServerStream pipeServer = null;
        NamedPipeData connect;

        public event Action<string> ReadData;
        public event EventHandler JavaClientClose;

        private const int InBufferSize = 128;
        private const int OutBufferSize = 128;

        private bool disposing = false;
        private bool clientMayDisconnected = false;


        private byte[] CreateSendBytes(string value) {
            var body = Encoding.UTF8.GetBytes(value);
            var sizeInfo = BitConverter.GetBytes(body.Length).ToList();
            sizeInfo.Reverse();
            sizeInfo.AddRange(body);
            body = null;
            return sizeInfo.ToArray();
        }

        public bool SendRenderRequest(string value) {
            if (disposing) return false;

            if (pipeServer != null && pipeServer.IsConnected) {
                var write = new NamedPipeWriteData(connect);
                write.pipe = pipeServer;
                write.offset = 0;
                write.writedata = CreateSendBytes(value + "\n\n");
                pipeServer.BeginWrite(write.writedata, 0, Math.Min(write.writedata.Length, OutBufferSize), BeginWriteCallback, write);
                pipeServer.WaitForPipeDrain();
                return true;
            }
            return false;
        }

        public Action<string> LogCallback = null;

        private void PutLog(string value) {
            if (!disposing) { // IsDisposed does not work expectedly, When close form without client close.
                if(LogCallback != null) {
                    try {
                        LogCallback(value);
                    }catch { }
                }
            }
        }


        public void Dispose() {
            if (!disposing) {
                disposing = true;
                if (pipeServer != null && pipeServer.IsConnected) {
                    try {
                        pipeServer.Close();
                    }
                    catch (Exception) { }
                }
            }
        }

        class NamedPipeData {
            public NamedPipeServerStream pipe;
            public bool IsDiscard = false;
        };


        class NamedPipeReadData {
            public NamedPipeReadData(NamedPipeData connect) {
                this.connect = connect;
            }
            public NamedPipeServerStream pipe;
            public NamedPipeData connect;
            public int TotalSize;
            public byte[] readThisTime;
            public List<byte> readdata = new List<byte>();
        };

        class NamedPipeWriteData {
            public NamedPipeWriteData(NamedPipeData connect) {
                this.connect = connect;
            }
            public NamedPipeServerStream pipe;
            public NamedPipeData connect;
            public int offset;
            public byte[] writedata;
        };

        private Process currentProcess;

        internal void RunClient() {
            var java = LibLocations.Java + "\\java.exe";
            var jar = LibLocations.Jar;

            var p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;

            p.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            p.StartInfo.FileName = java;
            p.StartInfo.Arguments = " -jar " + jar + " VSDebug=true";
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.StandardOutputEncoding = new UTF8Encoding();
            p.OutputDataReceived += (sender, args) => {
                                        Debug.WriteLine(args.Data);
                                    };
            p.ErrorDataReceived += (sender, args) => {
                                       Debug.WriteLine("error: " + args.Data);
                                   };

            if (!p.Start()) {
                Debug.Print(p.StandardOutput.ReadToEnd());
                Debug.Print(p.StandardError.ReadToEnd());
            }
            else {
                p.EnableRaisingEvents = true;
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
                p.Exited += (sender, args) => {
                    Debug.WriteLine("client process exited.");
                    // restart server.
                    StartServer();
                };
            }
        }


        private bool isAlreadyConnected = false;
        private string que = "";

        internal void StartServer() {
            if (pipeServer != null /* && pipeServer.IsConnected */) {
                pipeServer.Close();
                pipeServer = null;
            }
            if (connect != null) {
                connect.pipe = null;
                connect.IsDiscard = true;
            }

            currentProcess = Process.GetCurrentProcess();
            if (currentProcess.ProcessName.EndsWith("vshost")) {
                pipeServer = new NamedPipeServerStream(NamedPipeServerNamePrefix, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            }
            else {
                pipeServer = new NamedPipeServerStream(NamedPipeServerNamePrefix + "-" + currentProcess.Id, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            }
            connect = new NamedPipeData();
            connect.pipe = pipeServer;

            PutLog("[PIPE SERVER] thread created");
            isAlreadyConnected = false;
            que = "";
            pipeServer.BeginWaitForConnection(BeginWaitForConnectionCallback, connect);
        }



        private void BeginWaitForConnectionCallback(IAsyncResult ar) {
            var data = (NamedPipeData)ar.AsyncState;
            if (data.pipe != null && !data.IsDiscard) {
                PutLog("[PIPE SERVER] Client Connected");
                data.pipe.EndWaitForConnection(ar);

                var read = new NamedPipeReadData(connect);
                read.pipe = data.pipe;
                read.readThisTime = new byte[InBufferSize];
                read.TotalSize = 0;
                data.pipe.BeginRead(read.readThisTime, 0, InBufferSize, BeginReadCallback, read);

                var write = new NamedPipeWriteData(connect);
                write.pipe = data.pipe;
                write.offset = 0;
                write.writedata = CreateSendBytes("+OK Accepted.\n");
                data.pipe.BeginWrite(write.writedata, write.offset, write.writedata.Length, BeginWriteCallback, write);
            }
        }

        private void BeginWriteCallback(IAsyncResult ar) {
            var pd = (NamedPipeWriteData)ar.AsyncState;

            if (pd.pipe != null && pd.pipe.IsConnected) {
                try {
                    PutLog("[PIPE SERVER] Write Finished");
                    if (pd.pipe.IsConnected) pd.pipe.EndWrite(ar);

                    if (pd.offset + OutBufferSize < pd.writedata.Length) {
                        pd.offset += OutBufferSize;
                        pd.pipe.BeginWrite(pd.writedata, pd.offset, Math.Min(OutBufferSize, (pd.writedata.Length - pd.offset)), BeginWriteCallback, pd);
                    }
                    else {
                        isAlreadyConnected = true;
                    }
                }
                catch (IOException ex) {
                    PutLog("[PIPE SERVER] " + ex);
                }
            }
        }

        private const int lengthOfSizeBytes = 4;

        private void BeginReadCallback(IAsyncResult ar) {
            var pd = (NamedPipeReadData)ar.AsyncState;
            if (pd.pipe != null) {

                if (!pd.pipe.IsConnected) {
                    var s = this.pipeServer;
                    this.pipeServer = null;
                    if (s != null && !pd.connect.IsDiscard) {
                        s.Close();
                    }
                    PutLog("[PIPE SERVER] Client may disconnect. ");
                    Debug.WriteLine("[PIPE SERVER] Client may disconnect. ");
                    clientMayDisconnected = true;
                    pd.pipe.Close();
                    if(JavaClientClose!=null) JavaClientClose(this, new EventArgs());
                }
                else {
                    int bytesRead = pd.pipe.EndRead(ar);
                    if (bytesRead != 0) {
                        if (pd.TotalSize == 0) {
                            byte[] ts = pd.readThisTime.Take(lengthOfSizeBytes).Reverse().ToArray();
                            pd.TotalSize = BitConverter.ToInt32(ts, 0);
                            pd.readdata.AddRange(pd.readThisTime.Skip(lengthOfSizeBytes).Take(bytesRead - lengthOfSizeBytes).ToList());
                        }
                        else {
                            pd.readdata.AddRange(pd.readThisTime.Take(bytesRead).ToList());
                        }
                        pd.readThisTime = new byte[0];

                        if (pd.readdata.Count == pd.TotalSize) {
                            var s = Encoding.UTF8.GetString(pd.readdata.ToArray());
                            if (ReadData != null) ReadData(s);
                            var read = new NamedPipeReadData(connect);
                            read.pipe = pd.pipe;
                            read.readThisTime = new byte[InBufferSize];
                            read.TotalSize = 0;
                            pd.pipe.BeginRead(read.readThisTime, 0, InBufferSize, BeginReadCallback, read);
                        }
                        else {
                            if (pd.readdata.Count >= pd.TotalSize) {
                                // includes next data.
                                byte[] ts = pd.readdata.Skip(pd.TotalSize).Take(lengthOfSizeBytes).Reverse().ToArray();
                                byte[] next = new byte[pd.readdata.Count - pd.TotalSize - lengthOfSizeBytes];
                                next = pd.readdata.Skip(pd.TotalSize + lengthOfSizeBytes).ToArray();
                                pd.readdata.RemoveRange(pd.TotalSize, pd.readdata.Count - pd.TotalSize);

                                var s = Encoding.UTF8.GetString(pd.readdata.ToArray());
                                if (ReadData != null) ReadData(s);

                                var readNext = new NamedPipeReadData(connect);
                                readNext.pipe = pd.pipe;
                                readNext.readThisTime = new byte[InBufferSize];
                                readNext.TotalSize = BitConverter.ToInt32(ts, 0);
                                readNext.readdata.AddRange(next);
                                pd.pipe.BeginRead(readNext.readThisTime, 0, InBufferSize, BeginReadCallback, readNext);
                                return;
                            }
                            else {
                                pd.readThisTime = new byte[InBufferSize];
                                pd.pipe.BeginRead(pd.readThisTime, 0, InBufferSize, BeginReadCallback, pd);
                                return;
                            }

                        }
                    }
                }
            }
        }
    }
}
