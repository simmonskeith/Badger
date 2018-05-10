using Badger.Core;
using Badger.Core.Interfaces;
using Badger.Runner.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Badger.Runner.Presenters
{

    public class BadgerTestEditorPresenter : ITestEditorPresenter, IDisposable
    {
        private ITestEditorView _view;
        private IFileService _fileService;
        private IMessageBoxService _messageBox;
        private IFileBrowser _fileBrowser;
        private XDocument _doc;

        string LibraryName { get; set; }
        static string CurrentLibrary { get; set; }
        static Assembly CurrentAssembly { get; set; }
        static List<Type> StepClasses { get; set; } 

        public string FilePath { get; set; }

        public BadgerTestEditorPresenter(ITestEditorView view, 
            IFileService fileService, 
            IMessageBoxService messageBox,
            IFileBrowser fileBrowser)
        {
            _view = view;
            _fileService = fileService;
            _messageBox = messageBox;
            _fileBrowser = fileBrowser;
            _view.OnFormLoad += ViewOnFormLoad;
            _view.OnFormIsClosing += ViewOnFormClosing;
            _view.OnCloseClick += ViewOnCloseClick;
            _view.OnSaveClick += ViewOnSaveClick;
            _view.OnSaveAsClick += ViewOnSaveAsClick;
            _view.OnTestCaseTextChanged += ViewOnTestCaseChanged;
            _view.OnTreeNodeSelect += ViewOnTreeNodeSelect;
            _view.OnAddClick += ViewOnAddClicked;
        }

        public bool ShowView()
        {
            return _view.ShowDialog();
        }

        public void Dispose()
        {
            _view.OnFormLoad -= ViewOnFormLoad;
            _view.OnFormIsClosing -= ViewOnFormClosing;
            _view.OnCloseClick -= ViewOnCloseClick;
            _view.OnSaveClick -= ViewOnSaveClick;
            _view.OnSaveAsClick -= ViewOnSaveAsClick;
            _view.OnTestCaseTextChanged -= ViewOnTestCaseChanged;
            _view.OnTreeNodeSelect -= ViewOnTreeNodeSelect;
            _view.OnAddClick -= ViewOnAddClicked;
        }

        public void ViewOnFormLoad(object sender, EventArgs e)
        {
            _view.SaveEnabled = false;
            _view.AddStepEnabled = false;
            _view.ClearTreeNodes();
            // is there a file path?  if yes, get the library name and load it
            if (!string.IsNullOrEmpty(FilePath))
            {
                var reader = new TestFileReader(_fileService);
                reader.LoadFile(FilePath);
                LibraryName = reader.GetLibraryName();

                _view.TestCaseLines = _fileService.GetLines(FilePath).ToArray();
                _view.SaveAsEnabled = true;
            }
            else
            {
                _fileBrowser.InitialDirectory = Environment.CurrentDirectory;
                if (_fileBrowser.ShowDialog())
                {
                    LibraryName = System.IO.Path.GetFileNameWithoutExtension(_fileBrowser.FileName);
                }
                else
                {
                    //_view.Close();
                    _view.Result = DialogResult.Cancel;
                    
                }
                _view.TestCaseLines = new string[] { };
                _view.SaveAsEnabled = false;
            }
            SetTitle();

            if (String.IsNullOrEmpty(LibraryName) == false && CurrentLibrary != LibraryName)
            {
                // need to load new step list
                CurrentAssembly = Assembly.Load(LibraryName);
                CurrentLibrary = LibraryName;
                StepClasses = CurrentAssembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(StepsAttribute))).ToList();
            }

            // for the descriptions of the test steps
            LoadXml();
            BuildTestStepTree();

            _view.SaveEnabled = false;

        }

        private void BuildTestStepTree()
        {
            var root = _view.AddTreeNode("Steps");
            if (StepClasses != null)
            {
                foreach (var stepClass in StepClasses)
                {
                    TreeNode t = new TreeNode(stepClass.GetCustomAttribute<StepsAttribute>().Text);
                    var methods = stepClass.GetMethods()
                        .Where(m => Attribute.IsDefined(m, typeof(StepAttribute))).ToList();
                    foreach (var method in methods)
                    {
                        TreeNode node = new TreeNode();
                        node.Text = method.GetCustomAttribute<StepAttribute>().Text;
                        node.Tag = method;
                        t.Nodes.Add(node);
                    }

                    root.Nodes.Add(t);
                }
            }
        }


        public void ViewOnCloseClick(object sender, EventArgs e)
        {
            _view.Close();
        }

        public void ViewOnSaveClick(object sender, EventArgs e)
        {
            // if the file path is empty, need a dialog to pick the path.
            if (String.IsNullOrEmpty(FilePath))
            {
                _fileBrowser.CheckFileExists = false;
                if (_fileBrowser.ShowDialog())
                {
                    FilePath = _fileBrowser.FileName;
                }
            }

            if (String.IsNullOrEmpty(FilePath) == false)
            {
                _fileService.WriteLines(FilePath, _view.TestCaseLines, false);
                _view.SaveEnabled = false;
                SetTitle();
            }
        }

        public void ViewOnSaveAsClick(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(FilePath) == false)
            {
                _fileBrowser.InitialDirectory = System.IO.Path.GetDirectoryName(FilePath);
            }
            _fileBrowser.CheckFileExists = false;
            if (_fileBrowser.ShowDialog())
            {
                FilePath = _fileBrowser.FileName;
                _fileService.WriteLines(FilePath, _view.TestCaseLines, false);
                _view.SaveEnabled = false;
                SetTitle();
            }
        }

        public void ViewOnTestCaseChanged(object sender, EventArgs e)
        {
            _view.SaveEnabled = true;
            _view.SaveAsEnabled = true;
        }

        public void ViewOnFormClosing(object sender, FormClosingEventArgs e)
        {
            if (_view.SaveEnabled)
            {
                var result = _messageBox.Show("Unsaved changes.  Close anyway?", "Unsaved Changes", System.Windows.Forms.MessageBoxButtons.YesNo);
                if (result == DialogResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        public void ViewOnTreeNodeSelect(object sender, TreeViewEventArgs e)
        {
            if (_view.SelectedTreeNode.Tag != null)
            {
                var method = _view.SelectedTreeNode.Tag as MethodInfo;
                
                var info = GetMethodInfo(method.DeclaringType, method.Name);
                string description = info.Description + Environment.NewLine + Environment.NewLine;
                if (info.ParameterDescriptions != null && info.ParameterDescriptions.Count > 0)
                {
                    description += "Inputs:" + Environment.NewLine;
                    foreach (var item in info.ParameterDescriptions)
                    {
                        description += "  " + item.Key + ": " + item.Value + Environment.NewLine;
                    }
                }
                _view.TestStepDescription = description;
                _view.AddStepEnabled = true;
                
            }
            else
            {
                _view.TestStepDescription = String.Empty;
                _view.AddStepEnabled = false;
            }
        }

        public void ViewOnAddClicked(object sender, EventArgs e)
        {
            if (_view.SelectedTreeNode.Tag != null)
            {
                var method = _view.SelectedTreeNode.Tag as MethodInfo;
                string newStep = method.GetCustomAttribute<StepAttribute>().Text + Environment.NewLine;
                var parameters = method.GetParameters();
                if (parameters.Count() > 0)
                {
                    parameters.ToList().ForEach(p => {  newStep += "    " + p.Name + "    " + Environment.NewLine; });
                }
                _view.TestCaseSelectionLength = 0;
                _view.TestCaseSelectionText = newStep;
            }
        }

        private void SetTitle()
        {
            var title = String.IsNullOrEmpty(FilePath) ? "untitled.txt" : System.IO.Path.GetFileName(FilePath);
            _view.Title = $"Test Editor - {title}";
        }

        private StepDescription GetMethodInfo(Type actionType, string methodName)
        {
            StepDescription info = new StepDescription();
            try
            {
                if (_doc == null)
                {
                    return info;
                }

                string path = $"//members/member[contains(@name, 'M:{actionType.Namespace}.{actionType.Name}.{methodName}')]";
                XElement methodElement = _doc.XPathSelectElement(path);
                if (methodElement != null)
                {
                    info.Description = methodElement.XPathSelectElement("summary").Value.Trim();
                    info.ParameterDescriptions = new Dictionary<string, string>();
                    foreach (var parameter in methodElement.Elements("param"))
                    {
                        try
                        {
                            info.ParameterDescriptions[parameter.Attribute("name").Value] = parameter.Value;
                        }
                        catch (ArgumentException e)
                        {
                            Console.WriteLine($"Error adding description.  Action: {actionType.Name}, Method: {methodName}, Parameter: {parameter.Attribute("name").Value}");
                            Console.WriteLine(e);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error reading method info.  Action: {actionType.Name}, Method: {methodName}");
                Console.WriteLine(e);
            }

            return info;
        }

        struct StepDescription
        {
            public string Description;
            public Dictionary<string, string> ParameterDescriptions;
        }

        private void LoadXml()
        {
            var path = CurrentLibrary + ".xml";
            _doc = new System.Xml.Linq.XDocument();
            if (_fileService.FileExists(path))
            {
                _doc = _fileService.LoadHtmlFile(path);
            }
        }
    }
}
