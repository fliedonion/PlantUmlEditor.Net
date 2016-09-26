using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseOfT.Net.PlantUMLClient {
    public class LibLocations {

        static LibLocations() {
            jar = Properties.Settings.Default.JarPath ?? @"D:\repos\PlantUmlEditor.Net\bgServer\out\artifacts\net_case_of_t_plant_uml_editor_net_bgrender_jar\net.case-of-t.plant-uml-editor-net-bgrender.jar";
            java = Properties.Settings.Default.JavaPath ?? @"C:\ProgramData\Oracle\Java\javapath\java.exe";
            inkScape = Properties.Settings.Default.InkScapePath ?? @"c:\Program Files\Inkscape\inkscape.exe";
            graphViz = Properties.Settings.Default.GraphVizPath ?? "";
        }

        private static string jar;
        private static string java;
        private static string inkScape;
        private static string graphViz;

        public static string Java {
            get { return java; }
            set {
                java = value;
                if (File.Exists(Path.Combine(java, "javaw.exe"))) {
                    Properties.Settings.Default.JavaPath = java;
                    Properties.Settings.Default.Save();
                }
            }
        }
        public static string InkScape {
            get { return inkScape;}
            set {
                inkScape = value;
                if (File.Exists(inkScape)) {
                    Properties.Settings.Default.InkScapePath = inkScape;
                    Properties.Settings.Default.Save();
                }
            }
        }
        public static string GraphViz {
            get { return graphViz; }
            set {
                graphViz = value;
                if (File.Exists(graphViz)) {
                    Properties.Settings.Default.GraphVizPath = graphViz;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public static string Jar {
            get { return jar; }
            set {
                jar = value;
                if (File.Exists(jar)) {
                    Properties.Settings.Default.JarPath = jar;
                    Properties.Settings.Default.Save();
                }
            }
        }

        public static bool JavaExists() {
            return File.Exists(Path.Combine(Java, "javaw.exe"));
        }
        public static bool InkScapeExists() {
            return File.Exists(InkScape);
        }
        public static bool GraphVizExists() {
            return File.Exists(GraphViz);
        }

    }
}
