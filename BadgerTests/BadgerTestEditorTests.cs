using Badger.Core.Interfaces;
using Badger.Runner.Interfaces;
using Badger.Runner.Presenters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Xunit;
using NSubstitute;

namespace Badger.Tests
{
    [Trait("Category", "Badger Test Editor")]
    public class BadgerTestEditorTests
    {
        private BadgerTestEditorPresenter _presenter;
        ITestEditorView _view;
        IFileService _fileService;
        IMessageBoxService _messageBox;
        IFileBrowser _fileBrowser;

        public BadgerTestEditorTests()
        {
            _view = Substitute.For<ITestEditorView>();
            _view.AddTreeNode(Arg.Any<string>()).Returns(new TreeNode());
            _view.When(v=>v.ShowDialog()).Do(x=> _view.OnFormLoad += Raise.EventWith(null, EventArgs.Empty));
            _fileService = Substitute.For<IFileService>();
            _messageBox = Substitute.For<IMessageBoxService>();
            _fileBrowser = Substitute.For<IFileBrowser>();

            _fileService.GetLines(Arg.Any<string>()).Returns(new List<string>() { "Line1", "Line2" });
            _presenter = new BadgerTestEditorPresenter(_view, _fileService, _messageBox, _fileBrowser);
        }

        [Theory]
        [InlineData("MyTestPath.txt")]
        [InlineData("")]
        public void TestEditor_WhenLoaded_DisplaysConrolsInDefaultState(string path)
        {
            _presenter.FilePath = path;
            _presenter.ShowView();

            Assert.False(_view.SaveEnabled);
            Assert.False(_view.AddStepEnabled);
            Assert.Equal(!String.IsNullOrEmpty(path), _view.SaveAsEnabled);
        }

        [Fact]
        public void TestEditor_ClickingCancel_ClosesForm()
        {
            _view.OnCloseClick += Raise.EventWith(null, EventArgs.Empty);
            _view.Received(1).Close();
        }

        [Fact]
        public void TestEditor_ClickingCancelWhenChangesExist_PromptsUserToConfirm()
        {
            _messageBox.Show(Arg.Any<string>(), Arg.Any<string>(), MessageBoxButtons.YesNo)
                .Returns(DialogResult.No);
            _view.SaveEnabled.Returns(true);

            _view.OnFormIsClosing += Raise.Event<FormClosingEventHandler>(null, new FormClosingEventArgs(CloseReason.UserClosing, false));

            _messageBox.Received(1).Show(Arg.Any<string>(), Arg.Any<string>(), MessageBoxButtons.YesNo);
            
        }

        [Theory]
        [InlineData("")]
        [InlineData(@"c:\temp\testcase.txt")]
        public void TestEditor_WhenOpened_DisplaysTestCaseContent(string path)
        {
            DefaultSetup();
            _presenter.FilePath = path;
            _fileBrowser.ShowDialog().Returns(true);
            _fileBrowser.FileName.Returns(Assembly.GetAssembly(typeof(TestServiceTests)).GetName().Name + ".dll");
            _view.AddTreeNode(Arg.Any<string>()).Returns(new TreeNode());
            _presenter.ShowView();
            _fileService.Received(String.IsNullOrEmpty(path) ? 0 : 2).GetLines(path);
            Assert.True(_view.TestCaseLines.Count() == (String.IsNullOrEmpty(path) ? 0 : 3));
        }
        
        [Fact]
        public void TestEditor_WhenSaveIsClicked_SavesTestCase()
        {
            _presenter.FilePath = "MyFile.txt";
            _view.SaveEnabled.Returns(true);
            _view.SaveAsEnabled.Returns(true);

            _view.OnSaveClick += Raise.EventWith(null, EventArgs.Empty);

            _fileService.Received(1).WriteLines(_presenter.FilePath, _view.TestCaseLines, false);
            Assert.True(_view.SaveAsEnabled);
            Assert.False(_view.SaveEnabled);

        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestEditor_SaveTestCasesAs_SavesNewFiles(bool cancel)
        {
            _presenter.FilePath = "MyFile.txt";
            _view.SaveEnabled.Returns(true);
            _view.SaveAsEnabled.Returns(true);
            _fileBrowser.ShowDialog().Returns(!cancel);
            _fileBrowser.FileName.Returns("NewTestFile.txt");

            _view.OnSaveAsClick += Raise.EventWith(null, EventArgs.Empty);
            _fileService.Received(cancel ? 0 : 1).WriteLines(_presenter.FilePath, _view.TestCaseLines, false);
            Assert.Equal(cancel ? "MyFile.txt" : "NewTestFile.txt", _presenter.FilePath);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void TestEditor_BrowseNewFile_SetsFilePath(bool cancel)
        {
            _presenter.FilePath = "";
            _fileBrowser.ShowDialog().Returns(!cancel);
            _fileBrowser.FileName.Returns("MyTestFile.txt");

            _view.OnSaveClick += Raise.EventWith(null, EventArgs.Empty);
            _fileService.Received(cancel ? 0 : 1).WriteLines(_presenter.FilePath, _view.TestCaseLines, false);
            Assert.Equal(cancel ? "" : "MyTestFile.txt", _presenter.FilePath);
        }

        [Fact]
        public void TestEditor_WhenUserChangesText_SaveIsEnabled()
        {
            _view.SaveEnabled.Returns(false);
            _view.SaveAsEnabled.Returns(false);

            _view.OnTestCaseTextChanged += Raise.EventWith(null, EventArgs.Empty);

            Assert.True(_view.SaveEnabled);
            Assert.True(_view.SaveAsEnabled);
        }

        [Fact]
        public void TestEditor_DisplaysSteps_ForExistingLibrary()
        {
            var root = DefaultSetup();
            _presenter.ShowView();

            Assert.True(root.Nodes.Count == 1);
        }

        [Fact]
        public void TestEditor_WhenUserSelectsTreeNodeWithoutInputs_TestDescriptionUpdates()
        {
            var root = DefaultSetup();

            _presenter.ShowView();
            _view.SelectedTreeNode.Returns(root.FirstNode.FirstNode);

            _view.OnTreeNodeSelect += Raise.Event<TreeViewEventHandler>(null, new TreeViewEventArgs(root.FirstNode.FirstNode));

            Assert.Equal("A sample test method, without any inputs\r\n\r\n", _view.TestStepDescription);
            Assert.True(_view.AddStepEnabled);
        }

        [Fact]
        public void TestEditor_WhenTreeNodeWithInputsIsSelected_TestDescriptionUpdates()
        {
            var root = DefaultSetup();
            _presenter.ShowView();
            _view.SelectedTreeNode.Returns(root.FirstNode.Nodes[4]);

            _view.OnTreeNodeSelect += Raise.Event<TreeViewEventHandler>(null, new TreeViewEventArgs(root.FirstNode.Nodes[4]));

            Assert.Equal("A test step with inputs.\r\n\r\nInputs:\r\n  a: the first parameter\r\n  b: the second parameter\r\n",
                _view.TestStepDescription);
            Assert.True(_view.AddStepEnabled);
        }
        
        private TreeNode DefaultSetup()
        {
            _fileService.GetLines(Arg.Any<string>()).Returns(
                new List<string>() { "*** Settings ***",
                    "Library    " + Assembly.GetAssembly(typeof(TestServiceTests)).GetName().Name,
                "*** Test Steps ***"});

            _presenter.FilePath = "MyTest.txt";

            _fileService.FileExists(Arg.Any<string>()).Returns(true);
            _fileService.LoadHtmlFile(Arg.Any<string>()).Returns(
                XDocument.Parse("<?xml version=\"1.0\"?>" +
                  "<doc>" +
                  "<assembly>" +
                    "<name>Badger.Tests</name>" +
                  "</assembly>" +
                  "<members>" +
                    "<member name=\"T:AC.FunctionalTests.Steps.InstallerSteps\">" +
                       "<summary>" +
                          "Steps related to installing Amazing Charts." +
                       "</summary>" +
                     "</member>" +
                     "<member name=\"M:Badger.Tests.SampleTests.StepWithInputs(System.String, System.String)\" >" +
                       "<summary>" +
                         "A test step with inputs." +
                       "</summary>" +
                       "<param name=\"a\">the first parameter</param >" +
                       "<param name=\"b\">the second parameter</param >" +
                     "</member>" +
                     "<member name=\"M:AC.FunctionalTests.Steps.InstallerSteps.UninstallAmazingCharts\" >" +
                       "<summary>" +
                          "Runs the Amazing Charts uninstaller." +
                       "</summary >" +
                     "</member > " +
                     "<member name =\"M:AC.FunctionalTests.Steps.InstallerSteps.RunTheInstaller\" >" +
                        "<summary>" +
                           "Runs the Amazing Charts install" +
                        "</summary>" +
                     "</member >" +
                     "<member name = \"M:AC.FunctionalTests.Steps.InstallerSteps.VerifyAmazingChartsIsInstalled\" >" +
                     "<summary>" +
                         "Starts Amazing Charts to verify it was successfully installed." +
                     "</summary>" +
                     "</member>" +
                     "<member name = \"M:Badger.Tests.SampleTests.FirstStep\" >" +
                        "<summary>" +
                           "A sample test method, without any inputs" +
                        "</summary>" +
                      "</member>" +
                    "</members>" +
               "</doc>"));

            var node = new TreeNode();
            _view.AddTreeNode(Arg.Any<string>()).Returns(node);
            return node;
        }

    }
}
