using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner.Interfaces
{
    public interface IBadgerGui
    {
        string TestPath { get; set; }
        string OutputPath { get; set; }
        string TestStatusText { get; set; }
        string ResourceFileText { get; set; }
        string TagsText { get; set; }
        string ExcludeTagsText { get; set; }
        Color TestStatusForeColor { get; set; }
        Color TestStatusBackColor { get; set; }

        bool RunButtonEnabled { get; set; }
        bool ViewOutputMenuItemEnabled { get; set; }
        bool ViewReportMenuItemEnabled { get; set; }
        bool SelectFileEnabled { get; set; }
        bool SelectTestFolderEnabled { get; set; }
        bool SelectOutputFolderEnabled { get; set; }
        bool NewTestMenuItemEnabled { get; set; }
        bool EditTestMenuItemEnabled { get; set; }

        bool ShowDialog();

        event EventHandler OnFormLoad;
        event EventHandler OnSelectTestFile;
        event EventHandler OnSelectTestFolder;
        event EventHandler OnSelectOutputFolder;
        event EventHandler OnSelectResourceFile;
        event EventHandler OnRun;
        event EventHandler OnViewOutput;
        event EventHandler OnViewReport;
        event EventHandler OnEditTest;
        event EventHandler OnNewTest;
    }
}
