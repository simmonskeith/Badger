using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner.Interfaces
{
    public interface IFolderBrowser
    {
        Environment.SpecialFolder RootFolder { get; set; }
        string Description { get; set; }
        string SelectedPath { get; set; }
        bool ShowDialog();
    }
}
