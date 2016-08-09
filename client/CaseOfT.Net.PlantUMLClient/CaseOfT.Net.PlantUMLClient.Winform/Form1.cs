using CaseOfT.Net.PlantUMLClient.Tcp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CaseOfT.Net.PlantUMLClient {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e) {
            var renderd =  new PlantUmlTcpClient().RenderRequest(textBox1.Text);

            // webBrowser1.DocumentText = "<html><body>Hello world</body></html>";
            webBrowser1.DocumentText = renderd;

        }
    }
}
