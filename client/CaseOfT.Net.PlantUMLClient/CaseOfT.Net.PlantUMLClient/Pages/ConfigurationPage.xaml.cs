using System;
using System.Collections.Generic;
using System.Linq;
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
using CaseOfT.Net.PlantUMLClient.ViewModel;

namespace CaseOfT.Net.PlantUMLClient {
    /// <summary>
    /// Configuration.xaml の相互作用ロジック
    /// </summary>
    public partial class Configuration : Page {
        public Configuration() {
            InitializeComponent();

            DataContext = ApplicationViewModels.LibraryLocationModel;
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            if(NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }
}
