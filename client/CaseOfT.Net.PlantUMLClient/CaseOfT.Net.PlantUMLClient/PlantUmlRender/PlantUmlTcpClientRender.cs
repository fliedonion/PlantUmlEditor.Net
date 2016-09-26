using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    [Obsolete()]
    class PlantUmlTcpClientRender : IPlantUmlRender {

        public void Initialize() {
            PlantUmlTcpClientRender.port = 3000;
        }

        private const string ipAddr = "127.0.0.1";
        private static int port;

        public RenderResult RenderRequest(string plantUml) {
            var result = new RenderResult();
            result.Status = RenderResult.RenderStatuses.Success;

            if (String.IsNullOrEmpty(plantUml)) {
                result.Result = "";
                return result;
            }

            using (var tcp = new TcpClient(ipAddr, PlantUmlTcpClientRender.port)) {
                if (tcp.Connected) {
                    var ns = tcp.GetStream();
                    ns.ReadTimeout = 10000;
                    ns.WriteTimeout = 10000;

                    var sendData = Encoding.UTF8.GetBytes(plantUml);

                    var sizeInfo = BitConverter.GetBytes(sendData.Length).ToList();
                    sizeInfo.Reverse();


                    ns.Write(sizeInfo.ToArray(), 0, 4);
                    ns.Write(sendData, 0, sendData.Length);

                    var resBytes = new byte[256];
                    var ms = new MemoryStream();
                    int resSize = 0;
                    do {
                        resSize = ns.Read(resBytes, 0, resBytes.Length);
                        if (resSize == 0) break;

                        ms.Write(resBytes, 0, resSize);

                    } while (ns.DataAvailable);

                    string returnValue = Encoding.UTF8.GetString(ms.ToArray());
                    ms.Close();
                    ns.Close();
                    result.Result = returnValue;

                    return result;
                }else {
                    result.Status = RenderResult.RenderStatuses.CannotCommunicate;
                    return result;
                }
            }

        }

    }
}
