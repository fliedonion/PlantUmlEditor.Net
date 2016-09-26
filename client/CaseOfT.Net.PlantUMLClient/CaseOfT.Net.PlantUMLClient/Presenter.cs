using CaseOfT.Net.PlantUMLClient.PlantUmlRender;
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

        public Presenter() {
            try {
                CreateRender().Initialize();
            }
            catch (Exception ex) {
                _test = ex.Message;
            }
        }


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


        public ICommand RenderTextCommand
        {
            get { return new DelegateCommand(RenderText); }
        }

        private void RenderText() {
            AddToHistory(_someText);

            var thisText = _someText;
            new Task(() => {
                var rendered = CreateRender().RenderRequest(_someText??"");
                if (rendered.Status == RenderResult.RenderStatuses.Success) {
                    Test = rendered.Result;
                }
                // TODO: Implement other status.
            }).Start();
        }


        private IPlantUmlRender CreateRender() {
            // return new PlantUmlTcpClient();
            return new PlantUmlNamedPipeRender();
        }

        private void AddToHistory(string item) {
            if (!_history.Contains(item))
                _history.Add(item);
        }
    }
}
