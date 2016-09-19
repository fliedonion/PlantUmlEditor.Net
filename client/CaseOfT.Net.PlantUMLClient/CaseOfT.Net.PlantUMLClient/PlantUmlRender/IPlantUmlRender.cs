using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient.PlantUmlRender {
    interface IPlantUmlRender {
        string RenderRequest(string plantUmlSource);
    }
}
