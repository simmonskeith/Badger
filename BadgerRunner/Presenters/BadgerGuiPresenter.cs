using Badger.Core;
using Badger.Core.Interfaces;
using Badger.Runner.Interfaces;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Badger.Runner.Presenters
{

    public class BadgerGuiPresenter : IBadgerGuiPresenter, IDisposable
    {
        private IBadgerGui _view;
        private IFolderBrowser _folderBrowser;
        private IFileBrowser _fileBrowser;
        private IFileService _fileService;
        private IReportView _reportView;
        private IMessageBoxService _messageBox;
        private ITestEditorView _editor;
        private IReportViewPresenter _reportViewPresenter;
        private ITestEditorPresenter _testEditorPresenter;
        private string _resourcePath;

        Color COLOR_NOT_RUN = Color.SkyBlue;
        Color COLOR_CONTROL_TEXT = Color.Black;
        Color COLOR_PASS = Color.Lime;
        Color COLOR_RUNNING = Color.Yellow;
        Color COLOR_FAIL = Color.Red;

        const string MSG_NOT_RUN = "No Results";
        const string MSG_RUNNING = "RUNNING";
        const string MSG_PASS = "PASS";
        const string MSG_FAIL = "FAIL";
        

        public BadgerGuiPresenter(
            IBadgerGui view, 
            IFileService fileService, 
            IFolderBrowser folderBrowser, 
            IFileBrowser fileBrowser,
            IReportView reportView,
            IMessageBoxService messageBox,
            ITestEditorView editorView,
            ITestEditorPresenter testEditorPresenter,
            IReportViewPresenter reportViewPresenter)
        {
            _view = view;
            _fileService = fileService;
            _folderBrowser = folderBrowser;
            _fileBrowser = fileBrowser;
            _reportView = reportView;
            _messageBox = messageBox;
            _editor = editorView;
            _testEditorPresenter = testEditorPresenter;
            _reportViewPresenter = reportViewPresenter;

            _view.OnFormLoad += ViewOnFormLoad;
            _view.OnSelectTestFile += ViewOnSelectTestFile;
            _view.OnSelectTestFolder += ViewOnSelectTestFolder;
            _view.OnSelectOutputFolder += ViewOnSelectOutputFolder;
            _view.OnSelectResourceFile += ViewOnSelectResourceFile;
            _view.OnRun += ViewOnRun;
            _view.OnViewOutput += ViewOnViewOutput;
            _view.OnViewReport += ViewOnViewReport;
            _view.OnNewTest += ViewOnNewTest;
            _view.OnEditTest += ViewOnEditTest;
        }

        public void Dispose()
        {
            _view.OnFormLoad -= ViewOnFormLoad;
            _view.OnSelectTestFile -= ViewOnSelectTestFile;
            _view.OnSelectTestFolder -= ViewOnSelectTestFolder;
            _view.OnSelectOutputFolder -= ViewOnSelectOutputFolder;
            _view.OnSelectResourceFile -= ViewOnSelectResourceFile;
            _view.OnRun -= ViewOnRun;
            _view.OnViewOutput -= ViewOnViewOutput;
            _view.OnViewReport -= ViewOnViewReport;
            _view.OnNewTest -= ViewOnNewTest;
            _view.OnEditTest -= ViewOnEditTest;
        }

        public bool ShowView()
        {
            return _view.ShowDialog();
        }

        public string ResourcePath
        {
            get { return _resourcePath; }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    _resourcePath = Properties.Settings.Default.ResourcePath;
                }
                else
                {
                    _resourcePath = value;
                    Properties.Settings.Default.ResourcePath = value;
                    Properties.Settings.Default.Save();
                }
            }
        }

        private void ViewOnFormLoad(object sender, EventArgs e)
        {
            _view.TestPath = Properties.Settings.Default.TestPath;
            _view.OutputPath = Properties.Settings.Default.OutputPath;

            _view.RunButtonEnabled = false;
            _view.EditTestMenuItemEnabled = false;
            _view.ViewReportMenuItemEnabled = false;
            _view.ViewOutputMenuItemEnabled = false;
            _view.TestStatusText = MSG_NOT_RUN;
            _view.TestStatusBackColor = COLOR_NOT_RUN;
            _view.TestStatusForeColor = COLOR_CONTROL_TEXT;
            if (String.IsNullOrEmpty(_view.TestPath) == false)
            {
                _view.RunButtonEnabled = true;
                _view.EditTestMenuItemEnabled = !_fileService.IsDirectory(_view.TestPath);
            }

            UpdateResourceFileLabel();
            
        }

        private void ViewOnSelectTestFile(object sender, EventArgs e)
        {
            if (!String.IsNullOrEmpty(_view.TestPath))
            {
                if (_fileService.FileExists(_view.TestPath))
                {
                    _fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(_view.TestPath);
                }
                else
                {
                    _fileBrowser.InitialDirectory = _view.TestPath;
                }
            }
            else
            {
                _fileBrowser.InitialDirectory = Environment.CurrentDirectory;
            }

            if (_fileBrowser.ShowDialog())
            {
                UpdateTestPath(_fileBrowser.FileName);
                _view.EditTestMenuItemEnabled = true;
                if (String.IsNullOrEmpty(_view.OutputPath))
                {
                    UpdateOutputPath(Environment.CurrentDirectory);
                }
                UpdateControlsAfterFilePathChange(true);
            }
        }

        private void ViewOnSelectResourceFile(object sender, EventArgs e)
        {
            _fileBrowser.InitialDirectory = Environment.CurrentDirectory;
            if (!String.IsNullOrEmpty(ResourcePath) && _fileService.FileExists(ResourcePath))
            {
                _fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(ResourcePath);
 
            }

            if (_fileBrowser.ShowDialog())
            {
                ResourcePath = _fileBrowser.FileName;
                UpdateResourceFileLabel();
            }
        }

        private void UpdateResourceFileLabel()
        {
            if (string.IsNullOrEmpty(ResourcePath))
            {
                _view.ResourceFileText = "";
                _view.ResourceFileLabelVisible = false;
            }
            else
            {
                _view.ResourceFileText = $"Resource File: {ResourcePath}";
                _view.ResourceFileLabelVisible = true;
            }
        }

        private void ViewOnSelectTestFolder(object sender, EventArgs e)
        {
            _folderBrowser.RootFolder = Environment.SpecialFolder.Desktop;
            _folderBrowser.Description = "Select the folder containing tests.";

            // set the initial directory for the browser dialog
            if (!String.IsNullOrEmpty(_view.TestPath))
            {
                // if it's a file, use parent folder
                if (_fileService.FileExists(_view.TestPath))
                {
                    _folderBrowser.SelectedPath = System.IO.Path.GetDirectoryName(_view.TestPath);
                }
                else
                {
                    _folderBrowser.SelectedPath = _view.TestPath;
                }
            }
            else
            {
                // no path specified, so start from current directory
                _folderBrowser.SelectedPath = Environment.CurrentDirectory;
            }

            if (_folderBrowser.ShowDialog())
            {
                UpdateTestPath(_folderBrowser.SelectedPath);
                _view.EditTestMenuItemEnabled = false;
                // with test path selected, if an output path hasn't been set, use current directory.
                if (String.IsNullOrEmpty(_view.OutputPath))
                {
                    UpdateOutputPath(Environment.CurrentDirectory);
                }
                UpdateControlsAfterFilePathChange(true);
            }
        }

        private void UpdateTestPath(string path)
        {
            _view.TestPath = path;
            // save as the default setting for future use
            Properties.Settings.Default.TestPath = path;
            Properties.Settings.Default.Save();
        }

        private void UpdateOutputPath(string path)
        {
            _view.OutputPath = path;
            // save as the default setting for future use
            Properties.Settings.Default.OutputPath = path;
            Properties.Settings.Default.Save();
        }

        private void ViewOnSelectOutputFolder(object sender, EventArgs e)
        {
            _folderBrowser.RootFolder = Environment.SpecialFolder.Desktop;
            _folderBrowser.Description = "Select the folder for test reporting.";
            if (!String.IsNullOrEmpty(_view.OutputPath))
            {
                _folderBrowser.SelectedPath = _view.OutputPath;
            }
            else
            {
                _folderBrowser.SelectedPath = Environment.CurrentDirectory;
            }
            if (_folderBrowser.ShowDialog())
            {
                UpdateOutputPath(_folderBrowser.SelectedPath);
                UpdateControlsAfterFilePathChange(false);
            }
        }

        private async void ViewOnRun(object sender, EventArgs e)
        {
            _view.ViewOutputMenuItemEnabled = false;
            _view.ViewReportMenuItemEnabled = false;
            _view.RunButtonEnabled = false;
            _view.SelectTestFolderEnabled = false;
            _view.SelectOutputFolderEnabled = false;
            _view.SelectFileEnabled = false;
            _view.TestStatusText = MSG_RUNNING;
            _view.TestStatusBackColor = COLOR_RUNNING;
            _view.TestStatusForeColor = COLOR_CONTROL_TEXT;

            bool result = await RunTest();

            _view.TestStatusText = result ? MSG_PASS : MSG_FAIL;
            _view.TestStatusBackColor = result ? COLOR_PASS : COLOR_FAIL;
            _view.TestStatusForeColor = COLOR_CONTROL_TEXT;
            _view.ViewOutputMenuItemEnabled = true;
            _view.ViewReportMenuItemEnabled = true;
            _view.RunButtonEnabled = true;
            _view.SelectFileEnabled = true;
            _view.SelectOutputFolderEnabled = true;
            _view.SelectTestFolderEnabled = true;
            
        }

        private Task<bool> RunTest()
        {
            var runner = new TestRunner(new TestService(_fileService), _fileService);
            return Task.Run<bool>(() => runner.RunTests(_view.TestPath, _view.OutputPath, ResourcePath));
        }

        private void UpdateControlsAfterFilePathChange(bool includeRunButton)
        {
            _view.TestStatusText = "No Results";
            _view.TestStatusBackColor = COLOR_NOT_RUN;
            _view.TestStatusForeColor = COLOR_CONTROL_TEXT;
            if (includeRunButton)
            {
                _view.RunButtonEnabled = true;
            }
            _view.ViewOutputMenuItemEnabled = false;
            _view.ViewReportMenuItemEnabled = false;
        }

        private void ViewOnViewOutput(object sender, EventArgs e)
        {
            _fileService.StartProcess(System.IO.Path.Combine(_view.OutputPath, "Results"));
        }

        private void ViewOnViewReport(object sender, EventArgs e)
        {
            _reportViewPresenter.Url = System.IO.Path.Combine(_view.OutputPath, "Results", "summary.html");
            _reportViewPresenter.ShowView();
        }

        private void ViewOnNewTest(object sender, EventArgs e)
        {
            _testEditorPresenter.FilePath = String.Empty;
            _testEditorPresenter.ShowView();
            if (!string.IsNullOrEmpty(_testEditorPresenter.FilePath))
            {
                _view.TestPath = _testEditorPresenter.FilePath;
            }
        }

        private void ViewOnEditTest(object sender, EventArgs e)
        {
            _testEditorPresenter.FilePath = _view.TestPath;
            _testEditorPresenter.ShowView();
            if (_view.TestPath != _testEditorPresenter.FilePath)
            {
                _view.TestPath = _testEditorPresenter.FilePath;
            }
        }
            
    }
}
