using Badger.Runner.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Badger.Runner.Views
{
    public partial class ReportView : Form, IReportView
    {
        public ReportView()
        {
            InitializeComponent();
        }

        public new bool ShowDialog()
        {
            return base.ShowDialog() == DialogResult.OK;
        }

        public string Url
        {
            get { return webBrowser.Url.ToString(); }
        }

        public bool BackButtonEnabled
        {
            get { return backButton.Enabled; }
            set { backButton.Enabled = value; }
        }

        public bool ForwardButtonEnabled
        {
            get { return forwardButton.Enabled; }
            set { forwardButton.Enabled = value; }
        }

        public bool CanGoBack
        {
            get { return webBrowser.CanGoBack; }
        }

        public bool CanGoForward
        {
            get { return webBrowser.CanGoForward; }
        }

        public Size BrowserSize
        {
            get { return webBrowser.Size; }
        }

        public HtmlElement BrowserImage
        {
            get
            {
                if (webBrowser.Document.Images.Count > 0)
                {
                    return webBrowser.Document.Images[0];
                }
                else
                {
                    return null;
                }
            }
        }

        public event EventHandler CanGoBackChanged
        {
            add { this.webBrowser.CanGoBackChanged += value; }
            remove { this.webBrowser.CanGoBackChanged -= value; }
        }

        public event EventHandler CanGoForwardChanged
        {
            add { this.webBrowser.CanGoForwardChanged += value; }
            remove { this.webBrowser.CanGoForwardChanged -= value; }
        }

        public event EventHandler OnBackButtonClick
        {
            add { this.backButton.Click += value; }
            remove { this.backButton.Click -= value; }
        }

        public event EventHandler OnFormLoad
        {
            add { this.Load += value; }
            remove { this.Load -= value; }
        }

        public event EventHandler OnForwardButtonClick
        {
            add { this.forwardButton.Click += value; }
            remove { this.forwardButton.Click -= value; }
        }

        public event EventHandler OnHomeButtonClick
        {
            add { this.homeButton.Click += value; }
            remove { this.homeButton.Click -= value; }
        }

        public event EventHandler BrowserResized
        {
            add { this.webBrowser.SizeChanged += value; }
            remove { this.webBrowser.SizeChanged -= value; }
        }

        public event WebBrowserNavigatedEventHandler Navigated
        {
            add { this.webBrowser.Navigated += value; }
            remove { this.webBrowser.Navigated -= value; }
        }

        public void GoBack()
        {
            webBrowser.GoBack();
        }

        public void GoForward()
        {
            webBrowser.GoForward();
        }

        public void Navigate(string url)
        {
            webBrowser.Navigate(url);
        }
    }
}
