using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner.Interfaces
{
    public interface IReportView
    {
        bool ShowDialog();
        bool ForwardButtonEnabled { get; set; }
        bool BackButtonEnabled { get; set; }
        bool CanGoForward { get; }
        bool CanGoBack { get; }
        string Url { get; }
        System.Drawing.Size BrowserSize { get; }
        System.Windows.Forms.HtmlElement BrowserImage { get; }

        event EventHandler OnFormLoad;
        event EventHandler OnBackButtonClick;
        event EventHandler OnForwardButtonClick;
        event EventHandler OnHomeButtonClick;
        event EventHandler CanGoForwardChanged;
        event EventHandler CanGoBackChanged;
        event EventHandler BrowserResized;
        event System.Windows.Forms.WebBrowserNavigatedEventHandler Navigated;

        void Navigate(string url);
        void GoBack();
        void GoForward();

    }
}
