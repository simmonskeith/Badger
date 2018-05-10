using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Badger.Runner.Interfaces;

namespace Badger.Runner.Views
{
    public partial class BadgerGui : Form, IBadgerGui
    {
        public BadgerGui()
        {
            InitializeComponent();
        }

        public string TestPath
        {
            get { return lblTestPath.Text; }
            set { lblTestPath.Text = value; }
        }

        public string OutputPath
        {
            get { return lblOutputPath.Text; }
            set { lblOutputPath.Text = value; }
        }

        public string ResourceFileText
        {
            get { return lblResourceFile.Text; }
            set { lblResourceFile.Text = value; }
        }

        public bool RunButtonEnabled
        {
            get { return btnRun.Enabled; }
            set { btnRun.Enabled = value; }
        }

        public bool ViewOutputButtonEnabled
        {
            get { return viewOutputToolStripMenuItem.Enabled; }
            set { viewOutputToolStripMenuItem.Enabled = value; }
        }

        public bool ViewReportButtonEnabled
        {
            get { return viewReportToolStripMenuItem.Enabled; }
            set { viewReportToolStripMenuItem.Enabled = value; }
        }

        public bool SelectFileEnabled
        {
            get { return selectFileToolStripMenuItem.Enabled; }
            set { selectFileToolStripMenuItem.Enabled = value; }
        }

        public bool SelectTestFolderEnabled
        {
            get { return selectTestFolderToolStripMenuItem.Enabled; }
            set { selectTestFolderToolStripMenuItem.Enabled = value; }
        }

        public bool SelectOutputFolderEnabled
        {
            get { return selectOutputFolderToolStripMenuItem.Enabled; }
            set { selectOutputFolderToolStripMenuItem.Enabled = value; }
        }

        public bool NewTestEnabled
        {
            get { return newTestToolStripMenuItem.Enabled; }
            set { newTestToolStripMenuItem.Enabled = value; }
        }

        public bool EditTestEnabled
        {
            get { return editTestToolStripMenuItem.Enabled; }
            set { editTestToolStripMenuItem.Enabled = value; }
        }

        public string TestStatusText
        {
            get { return lblTestStatus.Text; }
            set
            {
                lblTestStatus.Text = value;
                lblTestStatus.Refresh();
            }
        }

        public Color TestStatusForeColor
        {
            get { return lblTestStatus.ForeColor; }
            set { lblTestStatus.ForeColor = value; }
        }

        public Color TestStatusBackColor
        {
            get { return lblTestStatus.BackColor; }
            set
            {
                lblTestStatus.BackColor = value;
                lblTestStatus.Refresh();
            }
        }

        public bool ResourceFileLabelVisible
        {
            get { return lblResourceFile.Visible; }
            set { lblResourceFile.Visible = value; }
        }

        public new bool ShowDialog()
        {
            return base.ShowDialog() == DialogResult.OK;
        }


        public event EventHandler OnFormLoad
        {
            add { this.Load += value; }
            remove { this.Load -= value; }
        }

        public event EventHandler OnSelectTestFile
        {
            add { selectFileToolStripMenuItem.Click += value; }
            remove { selectFileToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnSelectTestFolder
        {
            add { selectTestFolderToolStripMenuItem.Click += value; }
            remove { selectTestFolderToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnSelectOutputFolder
        {
            add { selectOutputFolderToolStripMenuItem.Click += value; }
            remove { selectOutputFolderToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnSelectResourceFile
        {
            add { selectResourceToolStripMenuItem.Click += value; }
            remove { selectResourceToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnRun
        {
            add { btnRun.Click += value; }
            remove { btnRun.Click -= value; }
        }

        public event EventHandler OnViewOutput
        {
            add { viewOutputToolStripMenuItem.Click += value; }
            remove { viewOutputToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnViewReport
        {
            add { viewReportToolStripMenuItem.Click += value; }
            remove { viewReportToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnEditTest
        {
            add { editTestToolStripMenuItem.Click += value; }
            remove { editTestToolStripMenuItem.Click -= value; }
        }

        public event EventHandler OnNewTest
        {
            add { newTestToolStripMenuItem.Click += value; }
            remove { newTestToolStripMenuItem.Click -= value; }
        }
    }
}
