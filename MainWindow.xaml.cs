using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Core;

namespace Kick_Streaming
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = true;
            Topmost = false;
            Loaded += MainWindow_Loaded;
            KeyDown += MainWindow_KeyDown;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var userDataFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "KickStreamingWebView2");
            var env = await CoreWebView2Environment.CreateAsync(null, userDataFolder);
            await webView.EnsureCoreWebView2Async(env);

            // Ensure profile persists cookies and login/autofill data
            var profile = webView.CoreWebView2.Profile;
            profile.IsPasswordAutosaveEnabled = true;
            profile.IsGeneralAutofillEnabled = true;

            webView.CoreWebView2.AddWebResourceRequestedFilter("*://*.doubleclick.net/*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.AddWebResourceRequestedFilter("*://*.googlesyndication.com/*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.AddWebResourceRequestedFilter("*://*.adservice.google.com/*", CoreWebView2WebResourceContext.All);
            webView.CoreWebView2.WebResourceRequested += CoreWebView2_WebResourceRequested;

            webView.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            webView.CoreWebView2.Navigate("https://kick.com/");
        }

        private void CoreWebView2_WebResourceRequested(object? sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            e.Response = webView.CoreWebView2.Environment.CreateWebResourceResponse(null, 403, "Blocked", "");
        }

        private async void CoreWebView2_NavigationCompleted(object? sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            string adBlockScript = @"
                (function() {
                    var adSelectors = [
                        '[id^=ad]', '[class*=ad]', '[class*=banner]', '[class*=sponsor]', '[class*=promo]', '[class*=ads]', '[class*=advert]', '[class*=doubleclick]', '[class*=googlesyndication]'
                    ];
                    adSelectors.forEach(function(selector) {
                        var elements = document.querySelectorAll(selector);
                        elements.forEach(function(el) { el.remove(); });
                    });
                })();
            ";
            await webView.ExecuteScriptAsync(adBlockScript);
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var core = webView.CoreWebView2;
                if (core != null && core.CanGoBack)
                {
                    core.GoBack();
                }
                e.Handled = true;
            }
        }
    }
}