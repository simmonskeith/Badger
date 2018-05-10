using Badger.Runner.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Badger.Runner.Helpers
{
    public class FolderBrowser : IFolderBrowser
    {
        FolderBrowserDialog _dlg;

        public FolderBrowser()
        {
            _dlg = new FolderBrowserDialog();
        }

        public Environment.SpecialFolder RootFolder
        {
            get { return _dlg.RootFolder; }
            set { _dlg.RootFolder = value; }
        }

        public string Description
        {
            get { return _dlg.Description; }
            set { _dlg.Description = value; }
        }

        public string SelectedPath
        {
            get { return _dlg.SelectedPath; }
            set { _dlg.SelectedPath = value; }
        }

        public bool ShowDialog()
        {
            return _dlg.ShowDialog() == DialogResult.OK;
        }
    }
}
