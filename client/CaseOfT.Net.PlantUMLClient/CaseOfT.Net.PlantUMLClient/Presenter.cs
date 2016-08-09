using CaseOfT.Net.PlantUMLClient.Tcp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CaseOfT.Net.PlantUMLClient {
    public class Presenter : ObservableObject {
        private string _someText;
        private readonly ObservableCollection<string> _history
            = new ObservableCollection<string>();

        private string _test;
        public string Test
        {
            get { return _test; }
            set { _test = value;
                RaisePropertyChangedEvent("Test");
            }
        }

        public string SomeText
        {
            get { return _someText; }
            set
            {
                _someText = value;
                RaisePropertyChangedEvent("SomeText");
            }
        }


        public IEnumerable<string> History
        {
            get { return _history; }
        }

        public ICommand CountTextCommand
        {
            get { return new DelegateCommand(CountText);  }
        }

        private void CountText() {
            // Test = _someText.Length.ToString();
        }


        public ICommand ReverseTextCommand
        {
            get { return new DelegateCommand(ReverseText); }
        }

        private void ReverseText() {
            AddToHistory(_someText);

            var thisText = _someText;
            new Task(() => {
                var renderd = new PlantUmlTcpClient().RenderRequest(_someText);
                Test = renderd;
            }).Start();
        }

        private void AddToHistory(string item) {
            if (!_history.Contains(item))
                _history.Add(item);
        }
    }
}
