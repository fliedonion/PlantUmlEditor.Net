using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;

namespace CaseOfT.Net.PlantUMLClient {
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window {
        private NavigationService navi = null;

        public MainWindow() {
            InitializeComponent();
            navi = mainFrame.NavigationService;

            Stream xshd_stream = File.OpenRead("plantUml.xshd");
            XmlTextReader xshd_reader = new XmlTextReader(xshd_stream);
            sourceEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            xshd_reader.Close();
            xshd_stream.Close();

            browser.NavigateToString("<html><body>Hello, World</body></html>");

        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            navi.Navigate(new Uri("Pages/EditorPage.xaml", UriKind.Relative));
        }
    }
}
