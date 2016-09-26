using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    class PlantUmlNamedPipeRender : IPlantUmlRender {

        public void Initialize() {
            InitServer();
        }

        internal PlantUmlNamedPipeRender() {
        }

        private static void InitServer() {
            if (server == null) {
                server = new PlantUmlNamedPipeServer();
                // server.RunClient();
                server.StartServer();
                server.JavaClientClose += (sender, args) => { server.StartServer(); };
            }
        }

        private static PlantUmlNamedPipeServer server = null;
        private static PlantUmlNamedPipeServer Server {
            get {
                InitServer();
                return server;
            }
        }

        public RenderResult RenderRequest(string plantUmlSource) {

            var t = FromNamedPipeServer(plantUmlSource);
            t.Wait();

            return t.Result;
        }

        private Task<RenderResult> FromNamedPipeServer(string plantUmlSource) {

            var tcs = new TaskCompletionSource<RenderResult>();
            Action<string> handler = s => {
                                         if (tcs.Task.Status != TaskStatus.RanToCompletion &&
                                                tcs.Task.Status != TaskStatus.WaitingForChildrenToComplete &&
                                                tcs.Task.Status != TaskStatus.Canceled &&
                                                tcs.Task.Status != TaskStatus.Faulted
                                            ) {
                                            var result = new RenderResult();
                                            result.Status = RenderResult.RenderStatuses.Success;
                                            result.Result = s;
                                            tcs.SetResult(result);
                                         } };
            Server.ReadData += handler;
            if (!Server.SendRenderRequest(plantUmlSource)) {
                var result = new RenderResult();
                result.Status = RenderResult.RenderStatuses.CannotCommunicate;
                result.Result = "Can't communicate java side";
                tcs.SetResult(result);
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
