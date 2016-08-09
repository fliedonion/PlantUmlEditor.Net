using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;

namespace CaseOfT.Net.PlantUMLClient {
    class WebBrowserBehavior{
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
                "Html",
                typeof(string),
                typeof(WebBrowserBehavior),
                new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d) {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value) {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e) {
            WebBrowser webBrowser = dependencyObject as WebBrowser;
            if (webBrowser != null) {
                var newValue = e.NewValue as string;
                if (string.IsNullOrEmpty(newValue)) {
                    newValue = "<html><body>nothing to render</body></html>";
                }
                webBrowser.NavigateToString(newValue);
            }
                
        }
    }
}
