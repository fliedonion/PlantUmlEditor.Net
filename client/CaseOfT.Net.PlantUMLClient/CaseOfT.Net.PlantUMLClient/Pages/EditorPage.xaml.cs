using System;
using System.Collections.Generic;
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
    }
}
