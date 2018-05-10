using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner.Interfaces
{
    public interface IFileBrowser
    {
        string InitialDirectory { get; set; }
        string FileName { get; set; }
        bool CheckFileExists { get; set; }
        bool ShowDialog();
    }
}
