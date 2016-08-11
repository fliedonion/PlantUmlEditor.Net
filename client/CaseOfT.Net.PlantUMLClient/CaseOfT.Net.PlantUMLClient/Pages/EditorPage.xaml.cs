using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
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
    /// Editor.xaml の相互作用ロジック
    /// </summary>
    public partial class Editor : Page {
        public Editor() {
            InitializeComponent();
            using (var xshd_stream = File.OpenRead("plantUml.xshd"))
            using (var xshd_reader = new XmlTextReader(xshd_stream)) {
                sourceEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd_reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);
            }

            browser.NavigateToString("<html><body>Enter Uml in Left Pane</body></html>");

            var dispatcher = Dispatcher;
            var keyUpStream = Observable.FromEvent<KeyEventHandler, KeyEventArgs>(
                h => (_, ev) => h(ev),
                h => sourceEditor.KeyUp += h,
                h => sourceEditor.KeyUp -= h);
            keyUpStream
                .Select(x => x.Key)
                .Throttle(new TimeSpan(0, 0, 0, 1))
                .Subscribe(x => {
                    dispatcher.Invoke(() => {
                        ((Presenter)this.DataContext).RenderTextCommand.Execute("Do not empty avoid Avast incorrect detect.");
                    });
                });

        }

        private void exportEmfButton_Click(object sender, RoutedEventArgs e) {
            try {
                var result = exportEmf();

                if (result != null && result == false) {
                    MessageBox.Show("出力に失敗しました。", "Warning", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }

            }
            catch (Exception ex) {
                MessageBox.Show("Export Error \r\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string lastExportDirectory = null;
        private bool? exportEmf() {

            var dlg = new SaveFileDialog();
            dlg.FilterIndex = 1;
            if(lastExportDirectory != null) {
                dlg.InitialDirectory = lastExportDirectory;
            }
            dlg.Title = "EMFファイルにエクスポートします";
            dlg.Filter = "EMF (*.emf)|*.emf|All Files(*.*)|*.*";
            dlg.OverwritePrompt = true;
            dlg.ValidateNames = true;
            bool? result = dlg.ShowDialog();
            if (result != null && result == true) {
                var filename = dlg.FileName;
                lastExportDirectory = new FileInfo(filename).Directory.FullName;
                return SaveToSvg(dlg.FileName);
            }
            return result;
        }



        private bool SaveToSvg(string saveFilename) {
            try {
                var tcp = new Tcp.PlantUmlTcpClient();
                var svg = tcp.RenderRequest(sourceEditor.Text);
                return ExportToEmf(svg, saveFilename);
            }
            catch (Exception ex) {
                throw new Exception("ローカルレンダリングサーバーと通信できませんでした。\r\n" + ex.Message, ex);
            }
        }

        private bool ExportToEmf(string svg, string saveFilename) {
            try {
                var tmpFile = System.IO.Path.GetTempFileName();
                File.WriteAllText(tmpFile, svg);
                var result = CallLinkScape(tmpFile, saveFilename);

                if (result.Item1 != 0) {
                    return false;
                }
                return true;
            }
            catch (Exception ex) {
                throw new Exception("一時ファイル作成に失敗しました。\r\n" + ex.Message, ex);
            }
        }

        private const string InkScapePath = @"c:\Program Files\Inkscape\inkscape.exe";
        private const string InkScapeArgFormat = @" ""{0}"" --export-emf=""{1}""";

        private Tuple<int, string, string> CallLinkScape(string tempSvgFile, string saveFilename) {

            try {
                string args = string.Format(InkScapeArgFormat, tempSvgFile, saveFilename);

                var pinfo = new ProcessStartInfo(InkScapePath, args);
                pinfo.UseShellExecute = false;
                pinfo.CreateNoWindow = false;
                pinfo.RedirectStandardOutput = true;
                pinfo.RedirectStandardError = true;

                var p = Process.Start(pinfo);
                var stdout = p.StandardOutput.ReadToEnd();
                var stderr = p.StandardError.ReadToEnd();
                p.WaitForExit();

                return Tuple.Create(p.ExitCode, stdout, stderr);

            }
            catch (Exception ex) {
                throw new Exception("inkscapeの起動で例外が発生しました。\r\n" + ex.Message, ex);
            }
        }

        private void configButton_Click(object sender, RoutedEventArgs e) {
            NavigationService.Navigate(new Uri("Pages/ConfigurationPage.xaml", UriKind.Relative));
        }

        private string lastOpenUmlDirectory = null;
        private void openButton_Click(object sender, RoutedEventArgs e) {
            var dlg = new OpenFileDialog();
            dlg.FilterIndex = 1;
            if(lastOpenUmlDirectory != null) {
                dlg.InitialDirectory = lastOpenUmlDirectory;
            }
            dlg.Title = "ロードします。";
            dlg.Filter = "plantuml (*.puml, *.plantuml)|*.puml;*.plantuml|All Files(*.*)|*.*";
            dlg.Multiselect = false;
            dlg.CheckFileExists = true;
            bool? result = dlg.ShowDialog();
            if (result != null && result == true) {
                sourceEditor.Text = File.ReadAllText(dlg.FileName, Encoding.UTF8);
                var filename = dlg.FileName;
                lastOpenUmlDirectory = new FileInfo(filename).Directory.FullName;
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e) {
            var dlg = new SaveFileDialog();
            dlg.FilterIndex = 1;
            if (lastOpenUmlDirectory != null) {
                dlg.InitialDirectory = lastOpenUmlDirectory;
            }
            dlg.Title = "テキストを保存します。";
            dlg.Filter = "plantuml (*.puml)|*.puml|All Files(*.*)|*.*";
            dlg.OverwritePrompt = true;
            dlg.ValidateNames = true;
            bool? result = dlg.ShowDialog();
            if (result != null && result == true) {
                File.WriteAllText(dlg.FileName, sourceEditor.Text, Encoding.UTF8);
                var filename = dlg.FileName;
                lastOpenUmlDirectory = new FileInfo(filename).Directory.FullName;
            }
        }
    }
}
