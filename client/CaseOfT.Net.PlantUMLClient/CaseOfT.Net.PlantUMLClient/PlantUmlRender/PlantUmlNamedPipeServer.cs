using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    class PlantUmlNamedPipeServer : IDisposable {

        private const string NamedPipeServerNamePrefix = "PlantUmlNPServer";

        NamedPipeServerStream pipeServer = null;
        NamedPipeData connect;

        private bool disposing = false;
        private bool clientMayDisconnected = false;

        public bool SendRenderRequest(string value) {
            if (disposing) return false;

            if (pipeServer != null && pipeServer.IsConnected) {
                var write = new NamedPipeWriteData(connect);
                write.pipe = pipeServer;
                write.offset = 0;
                write.writedata = Encoding.UTF8.GetBytes(value + "\n\n");

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
            public Object state;
            public Byte[] readdata;
        };

        class NamedPipeWriteData {
            public NamedPipeWriteData(NamedPipeData connect) {
                this.connect = connect;
            }
            public NamedPipeServerStream pipe;
            public NamedPipeData connect;
            public int offset;
            public Byte[] writedata;
        };


        internal void StartServer() {
            if (pipeServer != null /* && pipeServer.IsConnected */) {
                pipeServer.Close();
                pipeServer = null;
            }
            if (connect != null) {
                connect.pipe = null;
                connect.IsDiscard = true;
            }

            pipeServer = new NamedPipeServerStream(NamedPipeServerNamePrefix, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            connect = new NamedPipeData();
            connect.pipe = pipeServer;

            PutLog("[PIPE SERVER] thread created");
            pipeServer.BeginWaitForConnection(BeginWaitForConnectionCallback, connect);
        }

        public const int InBufferSize = 4096;
        public const int OutBufferSize = 4096;


        private void BeginWaitForConnectionCallback(IAsyncResult ar) {
            var data = (NamedPipeData)ar.AsyncState;
            if (data.pipe != null && !data.IsDiscard) {
                PutLog("[PIPE SERVER] Client Connected");
                data.pipe.EndWaitForConnection(ar);

                var read = new NamedPipeReadData(connect);
                read.pipe = data.pipe;
                read.readdata = new byte[InBufferSize];
                read.state = null;
                data.pipe.BeginRead(read.readdata, 0, InBufferSize, BeginReadCallback, read);

                var write = new NamedPipeWriteData(connect);
                write.pipe = data.pipe;
                write.offset = 0;
                write.writedata = Encoding.UTF8.GetBytes("+OK Accepted.\n");
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
                }
                catch (IOException ex) {
                    PutLog("[PIPE SERVER] " + ex);
                }
            }
        }

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
                }
                else {

                    int bytesRead = pd.pipe.EndRead(ar);
                    if (bytesRead != 0) {
                        // PutLog("[PIPE SERVER] Read: " + Encoding.UTF8.GetString(pd.readdata));
                        Debug.WriteLine(Encoding.UTF8.GetString(pd.readdata).Substring(0, 100));
                    }

                    var read = new NamedPipeReadData(connect);
                    read.pipe = pd.pipe;
                    read.readdata = new byte[InBufferSize];
                    read.state = null;
                    pd.pipe.BeginRead(read.readdata, 0, InBufferSize, BeginReadCallback, read);
                }
            }
        }
    }
}
