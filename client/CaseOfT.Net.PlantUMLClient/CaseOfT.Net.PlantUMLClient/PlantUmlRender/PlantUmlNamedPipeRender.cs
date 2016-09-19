using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    class PlantUmlNamedPipeRender : IPlantUmlRender {

        private static PlantUmlNamedPipeServer server = null;

        private static PlantUmlNamedPipeServer Server {
            get {
                if (server == null) {
                    server = new PlantUmlNamedPipeServer();
                    server.StartServer();
                    server.ReadData += s => { Debug.WriteLine(s); };
                    server.JavaClientClose += (sender, args) => { server.StartServer(); };
                }
                return server;
            }
        }

        public string RenderRequest(string plantUmlSource) {
            if (Server.SendRenderRequest(plantUmlSource)) {
                return string.Empty;
            }
            else {
                return "Can't communicate java side";
            }
        }
    }
}
