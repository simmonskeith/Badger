using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Badger.Runner.Interfaces
{
    public interface ITestEditorView
    {
        string Title { get; set; }
        DialogResult Result { get; set; }
        string[] TestCaseLines { get; set; }
        bool SaveEnabled { get; set; }
        bool SaveAsEnabled { get; set; }
        bool AddStepEnabled { get; set; }
        string TestStepDescription { get; set; }
        int TestCaseSelectionLength { get; set; }
        string TestCaseSelectionText { get; set; }

        TreeNode AddTreeNode(string name);
        TreeNode SelectedTreeNode { get; set; }
        TreeNodeCollection Nodes { get; }

        bool ShowDialog();
        void Close();
        void ClearTreeNodes();

        event EventHandler OnFormLoad;
        event EventHandler OnCloseClick;
        event FormClosingEventHandler OnFormIsClosing;
        event EventHandler OnSaveClick;
        event EventHandler OnSaveAsClick;
        event EventHandler OnTestCaseTextChanged;
        event TreeViewEventHandler OnTreeNodeSelect;
        event EventHandler OnAddClick;
    }
}
