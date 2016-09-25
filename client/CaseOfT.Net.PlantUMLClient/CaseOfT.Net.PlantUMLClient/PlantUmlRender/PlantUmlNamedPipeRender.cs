using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    class PlantUmlNamedPipeRender : IPlantUmlRender {

        internal PlantUmlNamedPipeRender() {
            var s = Server;
        }

        private static PlantUmlNamedPipeServer server = null;

        private static PlantUmlNamedPipeServer Server {
            get {
                if (server == null) {
                    server = new PlantUmlNamedPipeServer();
                    server.RunClient();
                    server.StartServer();
                    server.JavaClientClose += (sender, args) => { server.StartServer(); };
                }
                return server;
            }
        }

        public string RenderRequest(string plantUmlSource) {

            var t = FromNamedPipeServer(plantUmlSource);
            t.Wait();

            return t.Result;
        }

        private Task<string> FromNamedPipeServer(string plantUmlSource) {

            var tcs = new TaskCompletionSource<string>();
            Action<string> handler = s => { tcs.SetResult(s); };
            Server.ReadData += handler;
            if (!Server.SendRenderRequest(plantUmlSource)) {
                tcs.SetResult("Can't communicate java side");
                Server.ReadData -= handler;
                handler = null;
            }
            tcs.Task.ContinueWith(ts => {
                                        if (handler != null) {
                                            Server.ReadData -= handler;
                                        }
                                    });
            return tcs.Task;
        }

    }
}
