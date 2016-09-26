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

namespace CaseOfT.Net.PlantUMLClient {
    /// <summary>
    /// Configuration.xaml の相互作用ロジック
    /// </summary>
    public partial class Configuration : Page {
        public Configuration() {
            InitializeComponent();

            JavaLocation.Text = LibLocations.Java;
            GraphvizLocation.Text = LibLocations.GraphViz;
            InkScapeLocation.Text = LibLocations.InkScape;
        }

        private void button_Click(object sender, RoutedEventArgs e) {
            if(NavigationService.CanGoBack) NavigationService.GoBack();
        }

        private void JavaLocation_TextChanged(object sender, TextChangedEventArgs e) {
            LibLocations.Java = JavaLocation.Text;
        }

        private void GraphvizLocation_TextChanged(object sender, TextChangedEventArgs e) {
            LibLocations.GraphViz = GraphvizLocation.Text;
        }

        private void InkScapeLocation_TextChanged(object sender, TextChangedEventArgs e) {
            LibLocations.InkScape = InkScapeLocation.Text;
        }

        private void JarLocation_OnTextChangedLocation_TextChanged(object sender, TextChangedEventArgs e) {
            LibLocations.Jar = JarLocation.Text;
        }
    }
}
