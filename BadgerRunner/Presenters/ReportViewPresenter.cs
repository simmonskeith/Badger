using Badger.Runner.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Badger.Runner.Presenters
{

    public class ReportViewPresenter : IReportViewPresenter, IDisposable
    {
        public string Url { get; set; }
        string _homeUrl;
        IReportView _view;
        IMessageBoxService _okDialog;

        public ReportViewPresenter(IReportView view, IMessageBoxService okDialog)
        {
            _view = view;
            _okDialog = okDialog;
            _view.OnFormLoad += ViewOnFormLoad;
            _view.OnBackButtonClick += ViewOnBackButtonClick;
            _view.OnForwardButtonClick += ViewOnForwardButtonClick;
            _view.OnHomeButtonClick += ViewOnHomeButtonClick;
            _view.CanGoForwardChanged += ViewCanGoForwardChanged;
            _view.CanGoBackChanged += ViewCanGoBackChanged;
            _view.Navigated += ViewNavigated;
            _view.BrowserResized += ViewBrowserResized;
        }

        public void Dispose()
        {
            _view.OnFormLoad -= ViewOnFormLoad;
            _view.OnBackButtonClick -= ViewOnBackButtonClick;
            _view.OnForwardButtonClick -= ViewOnForwardButtonClick;
            _view.OnHomeButtonClick -= ViewOnHomeButtonClick;
            _view.CanGoForwardChanged -= ViewCanGoForwardChanged;
            _view.CanGoBackChanged -= ViewCanGoBackChanged;
            _view.Navigated -= ViewNavigated;
            _view.BrowserResized -= ViewBrowserResized;
        }

        public bool ShowView()
        {
            return _view.ShowDialog();
        }

        private void ViewOnFormLoad(object sender, EventArgs e)
        {
            _view.BackButtonEnabled = false;
            _view.ForwardButtonEnabled = false;
            if (String.IsNullOrEmpty(Url) == false)
            {
                _view.Navigate(Url);
                _homeUrl = Url;
            }
            else
            {
                _okDialog.Show("Unable to navigate.  The url provided is either null or empty.", "Navigation Error", MessageBoxButtons.OK);
            }
        }

        private void ViewOnBackButtonClick(object sender, EventArgs e)
        {
            _view.GoBack();
        }

        private void ViewOnForwardButtonClick(object sender, EventArgs e)
        {
            _view.GoForward();
        }

        private void ViewOnHomeButtonClick(object sender, EventArgs e)
        {
            _view.Navigate(_homeUrl);
        }

        private void ViewCanGoForwardChanged(object sender, EventArgs e)
        {
            _view.ForwardButtonEnabled = _view.CanGoForward;
        }

        private void ViewCanGoBackChanged(object sender, EventArgs e)
        {
            _view.BackButtonEnabled = _view.CanGoBack;
        }

        private void ViewNavigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            SetImageSize();
        }

        private void ViewBrowserResized(object sender, EventArgs e)
        {
            SetImageSize();
        }

        private void SetImageSize()
        {
            try
            {
                if (_view.BrowserImage != null)
                {
                    double origSizeHeight = _view.BrowserSize.Height;
                    double origSizeWidth = _view.BrowserSize.Width;

                    System.Windows.Forms.HtmlElement pic = _view.BrowserImage;
                    double origPicWidth = double.Parse(pic.GetAttribute("WIDTH"));
                    double origPicHeight = double.Parse(pic.GetAttribute("HEIGHT"));

                    double tempW, tempY, widthScale, heightScale;
                    double scale = 0;
                    if ((origPicWidth > origSizeWidth) || (origPicWidth < origSizeWidth))
                    {
                        widthScale = origSizeWidth / origPicWidth;
                        heightScale = origSizeHeight / origPicHeight;

                        scale = Math.Min(widthScale, heightScale);
                        tempW = origPicWidth * scale;
                        tempY = origPicHeight * scale;
                        pic.SetAttribute("WIDTH", tempW.ToString());
                        pic.SetAttribute("HEIGHT", tempY.ToString());
                    }

                }
            }
            catch (Exception) { }
        }
    }
}
