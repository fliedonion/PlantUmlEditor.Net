using PlantUmlEditor.Net.Tcp;
using System;
using System.Windows.Forms;

namespace PlantUmlEditor.Net {
    public partial class EditorForm : Form {
        public EditorForm() {
            InitializeComponent();
        }

        private void renderButton_Click(object sender, EventArgs e) {
            var renderd = new PlantUmlTcpClient().RenderRequest(umlText.Text);
            webBrowser1.DocumentText = renderd;
        }
    }
}
