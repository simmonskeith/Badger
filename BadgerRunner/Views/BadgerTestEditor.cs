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
    public partial class BadgerTestEditor : Form, ITestEditorView
    {
        public BadgerTestEditor()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return this.Text; }
            set { this.Text = value; }
        }

        public DialogResult Result
        {
            get { return this.DialogResult;  }
            set { this.DialogResult = value; }
        }

        public string[] TestCaseLines
        {
            get { return txtTestCase.Lines; }
            set { txtTestCase.Lines = value; }
        }

        public bool SaveEnabled
        {
            get { return saveToolStripMenuItem.Enabled; }
            set { saveToolStripMenuItem.Enabled = value; }
        }

        public bool SaveAsEnabled
        {
            get { return saveAsToolStripMenuItem.Enabled; }
            set { saveAsToolStripMenuItem.Enabled = value; }
        }

        public bool AddStepEnabled
        {
            get { return btnAdd.Enabled; }
            set { btnAdd.Enabled = value; }
        }

        public string TestStepDescription
        {
            get { return txtStepDesc.Text; }
            set { txtStepDesc.Text = value; }
        }

        public string TestCaseSelectionText
        {
            get { return txtTestCase.SelectedText; }
            set { txtTestCase.SelectedText = value; }
        }

        public int TestCaseSelectionLength
        {
            get { return txtTestCase.SelectionLength; }
            set { txtTestCase.SelectionLength = value; }
        }

        public TreeNodeCollection Nodes
        {
            get { return treeView1.Nodes; }
        }

        public TreeNode SelectedTreeNode
        {
            get { return treeView1.SelectedNode; }
            set { treeView1.SelectedNode = value; }
        }

        public TreeNode AddTreeNode(string text)
        {
            return treeView1.Nodes.Add(text);
        }

        public void ClearTreeNodes()
        {
            treeView1.Nodes.Clear();
        }

        public event EventHandler OnFormLoad
        {
            add { this.Load += value; }
            remove { this.Load -= value; }
        }

        public event FormClosingEventHandler OnFormIsClosing
        {
            add { this.FormClosing += value; }
            remove { this.FormClosing -= value; }
        }

        public event EventHandler OnCloseClick
        {
            add { closeToolStripMenuItem.Click += value; }
            remove { closeToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnSaveClick
        {
            add {saveToolStripMenuItem.Click += value; }
            remove { saveToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnSaveAsClick
        {
            add { saveAsToolStripMenuItem.Click += value; }
            remove { saveAsToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnAddClick
        {
            add { btnAdd.Click += value; }
            remove { btnAdd.Click -= value; }
        }

        public event EventHandler OnTestCaseTextChanged
        {
            add { txtTestCase.TextChanged += value; }
            remove { txtTestCase.TextChanged -= value; }
        }

        public event TreeViewEventHandler OnTreeNodeSelect
        {
            add { treeView1.AfterSelect += value; }
            remove { treeView1.AfterSelect -= value; }
        }

        public new bool ShowDialog()
        {
            return base.ShowDialog() == DialogResult.OK;
        }

        public new void Close()
        {
            base.Close();
        }
    }
}
