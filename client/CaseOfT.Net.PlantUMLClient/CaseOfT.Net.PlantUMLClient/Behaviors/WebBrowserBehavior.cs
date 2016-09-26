using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Navigation;

namespace CaseOfT.Net.PlantUMLClient.Behaviors {
    class WebBrowserBehavior : Behavior<WebBrowser> {
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

        protected override void OnAttached() {
            base.OnAttached();
            if (AssociatedObject != null)
                AssociatedObject.Navigating += AssociatedObjectOnNavigating;
        }
        protected override void OnDetaching() {
            base.OnDetaching();
            if (AssociatedObject != null)
                AssociatedObject.Navigating -= AssociatedObjectOnNavigating;
        }

        private void AssociatedObjectOnNavigating(object sender, NavigatingCancelEventArgs e) {
            if (e.Uri != null) {
                e.Cancel = true;
            }
        }

    }
}
