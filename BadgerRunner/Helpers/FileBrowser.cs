using Badger.Runner.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Badger.Runner.Helpers
{
    public class FileBrowser : IFileBrowser
    {
        private OpenFileDialog _dlg;

        public FileBrowser()
        {
            _dlg = new OpenFileDialog();
        }

        public string FileName
        {
            get { return _dlg.FileName; }

            set { _dlg.FileName = value; }
        }

        public string InitialDirectory
        {
            get { return _dlg.InitialDirectory; }
            set { _dlg.InitialDirectory = value; }
        }

        public bool ShowDialog()
        {
            return _dlg.ShowDialog() == DialogResult.OK;
        }
        
        public bool CheckFileExists
        {
            get { return _dlg.CheckFileExists; }
            set { _dlg.CheckFileExists = value; }
        }
    }
}
