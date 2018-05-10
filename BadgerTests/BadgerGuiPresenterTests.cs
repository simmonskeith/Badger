using Badger.Core.Interfaces;
using Badger.Runner.Interfaces;
using Badger.Runner.Presenters;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Badger.Tests
{
    [Trait("Category", "Badger GUI")]
    public class BadgerGuiPresenterTests
    {
        BadgerGuiPresenter presenter;
        IBadgerGui _view;
        IFileService _fileService;
        IFolderBrowser _folderBrowser;
        IFileBrowser _fileBrowser;
        IReportView _reportView;
        IReportViewPresenter _reportViewPresenter;
        IMessageBoxService _messageBox;
        ITestEditorView _editView;
        ITestEditorPresenter _editViewPresenter;

        private BadgerGuiPresenter InitPresenter(string resourcePath)
        {
            _view = Substitute.For<IBadgerGui>();
            _fileService = Substitute.For<IFileService>();
            _folderBrowser = Substitute.For<IFolderBrowser>();
            _fileBrowser = Substitute.For<IFileBrowser>();
            _messageBox = Substitute.For<IMessageBoxService>();

            _editView = Substitute.For<ITestEditorView>();
            _editViewPresenter = Substitute.For<ITestEditorPresenter>();
            _editViewPresenter.When(x=>x.ShowView()).Do(x=>_editView.ShowDialog());

            _reportView = Substitute.For<IReportView>();
            _reportViewPresenter = Substitute.For<IReportViewPresenter>();
            _reportViewPresenter.When(v=>v.ShowView()).Do(v=>_reportView.ShowDialog());

            var presenter = new BadgerGuiPresenter(
                _view,
                _fileService,
                _folderBrowser,
                _fileBrowser,
                _reportView,
                _messageBox,
                _editView,
                _editViewPresenter,
                _reportViewPresenter);
            presenter.ResourcePath = resourcePath;
            return presenter;
        }

        public BadgerGuiPresenterTests()
        {
            presenter = InitPresenter(null);
        }

        [Fact]
        public void BadgerGuiRunner_OnLoad_DisplaysDefaultState()
        {
            _view.NewTestEnabled.Returns(true);
            _view.EditTestEnabled.Returns(true);
            _view.OnFormLoad += Raise.EventWith(null, EventArgs.Empty);

            Assert.Equal(_view.RunButtonEnabled, !String.IsNullOrEmpty(_view.TestPath));
            Assert.False(_view.ViewOutputButtonEnabled);
            Assert.False(_view.ViewReportButtonEnabled);
            Assert.True(_view.NewTestEnabled);
            Assert.Equal(_view.EditTestEnabled, !String.IsNullOrEmpty(_view.TestPath));
            Assert.False(_view.ResourceFileLabelVisible);
        }
        
        [Theory]
        [InlineData("", false)]
        [InlineData(null, false)]
        [InlineData("c:\\myResourceFile.txt", true)]
        public void BadgerGuiRunner_Displays_ResourceFile(string filename, bool isVisible)
        {
            presenter = InitPresenter(filename);
            var expectedText = String.IsNullOrEmpty(filename) ? null : $"Resource File: {filename}";

            _view.OnFormLoad += Raise.EventWith(null, EventArgs.Empty);

            _view.Received(1).ResourceFileText = expectedText;
            _view.Received(1).ResourceFileLabelVisible = isVisible;
            Assert.Equal(expectedText, _view.ResourceFileText);
        }
        
        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        [InlineData("c:\\myFile.txt", true)]
        [InlineData("c:\\myFile.txt", false)]
        public void BadgerGuiRunner_Selects_ResourceFile(string originalPath, bool cancel)
        {
            presenter = InitPresenter(originalPath);
            var expected = cancel ? (String.IsNullOrEmpty(originalPath) ? null : $"Resource File: {originalPath}") :
                "Resource File: C:\\newFile.txt";
            _fileService.FileExists(Arg.Any<string>()).Returns(true);
            _fileBrowser.ShowDialog().Returns(!cancel);
            _fileBrowser.FileName.Returns("C:\\newFile.txt");
            _view.OnFormLoad += Raise.EventWith(null, EventArgs.Empty);

            _view.OnSelectResourceFile += Raise.EventWith(null, EventArgs.Empty);

            _view.Received(1).ResourceFileText = expected;
            Assert.Equal(expected, _view.ResourceFileText);

        }
        
        [Theory]
        [InlineData("", false)]
        [InlineData(@"c:\temp\tests\mytest.txt", true)]
        [InlineData(@"c:\temp\tests", false)]
        public void BadgerGuiRunner_ClickTestBrowse_BrowsesTestFile(string initialPath, bool isFile)
        {
            string newPath = @"c:\temp\tests\newtest.txt";
            _view.TestPath.Returns(initialPath);
            _view.ViewOutputButtonEnabled.Returns(true);
            _view.ViewReportButtonEnabled.Returns(true);
            _view.EditTestEnabled.Returns(false);
            _view.TestStatusText.Returns("PASS");
            _view.TestStatusBackColor.Returns(System.Drawing.Color.Green);
            _fileService.FileExists(initialPath).Returns(isFile);
            _fileBrowser.FileName.Returns(newPath);
            _fileBrowser.ShowDialog().Returns(true);

            // when no path is specified, use the current directory
            if (string.IsNullOrEmpty(initialPath))
            {
                initialPath = Environment.CurrentDirectory;
            }
            else if (isFile)
            {
                initialPath = System.IO.Path.GetDirectoryName(initialPath);
            }

            _view.OnSelectTestFile += Raise.EventWith(null, EventArgs.Empty);

            _fileBrowser.Received(1).ShowDialog();
            _fileBrowser.Received(1).InitialDirectory = initialPath;
            _view.Received(1).TestPath = newPath;

            Assert.False(_view.ViewReportButtonEnabled);
            Assert.False(_view.ViewOutputButtonEnabled);
            Assert.Equal("No Results", _view.TestStatusText);
            Assert.True(_view.EditTestEnabled);
            Assert.Equal(System.Drawing.Color.SkyBlue, _view.TestStatusBackColor);
        }
        
        [Fact]
        public void BadgerGuiRunner_BrowseTestFileCancel_DoesntChangePath()
        {
            string testPath = @"c:\temp\tests\mytest.txt";
            _view.TestPath.Returns(testPath);
            _view.ViewOutputButtonEnabled.Returns(true);
            _view.ViewReportButtonEnabled.Returns(true);
            _view.TestStatusText.Returns("PASS");
            _view.TestStatusBackColor.Returns(System.Drawing.Color.Green);
            _fileService.FileExists(testPath).Returns(true);
            _folderBrowser.ShowDialog().Returns(false);

            _view.OnSelectTestFile += Raise.EventWith(null, EventArgs.Empty);
            _view.DidNotReceive().TestPath = Arg.Any<string>();

            Assert.True(_view.ViewOutputButtonEnabled);
            Assert.True(_view.ViewReportButtonEnabled);
            Assert.Equal("PASS", _view.TestStatusText);
            Assert.Equal(System.Drawing.Color.Green, _view.TestStatusBackColor);
        }
        
        [Theory]
        [InlineData("", false)]
        [InlineData(@"c:\temp\tests\mytest.txt", true)]
        [InlineData(@"c:\temp\tests", false)]
        public void BadgerGuiRunner_ClickBrowseTestFolder_BrowsesCurrentTestFolder(string path, bool isFile)
        {
            _view.TestPath.Returns(path);
            _view.ViewOutputButtonEnabled.Returns(true);
            _view.ViewReportButtonEnabled.Returns(true);
            _view.TestStatusText.Returns("PASS");
            _view.EditTestEnabled.Returns(isFile);
            _view.TestStatusBackColor.Returns(System.Drawing.Color.Green);
            _fileService.FileExists(path).Returns(isFile);
            _folderBrowser.SelectedPath.Returns("intial path");
            _folderBrowser.ShowDialog().Returns(true);
            // when no path is specified, use the current directory
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.CurrentDirectory;
            }

            _view.OnSelectTestFolder += Raise.EventWith(null, EventArgs.Empty);

            _folderBrowser.Received(1).ShowDialog();
            _view.Received(1).TestPath = isFile ? System.IO.Path.GetDirectoryName(path) : path;

            Assert.False(_view.ViewReportButtonEnabled);
            Assert.False(_view.ViewOutputButtonEnabled);
            Assert.False(_view.EditTestEnabled);
            Assert.Equal("No Results", _view.TestStatusText);
            Assert.Equal(System.Drawing.Color.SkyBlue, _view.TestStatusBackColor);

        }
        
        [Fact]
        public void BadgerGuiRunner_BrowseTestFolderCancel_DoesntChangePath()
        {
            string testPath = @"c:\temp\tests";
            _view.TestPath.Returns(testPath);
            _view.ViewOutputButtonEnabled.Returns(true);
            _view.ViewReportButtonEnabled.Returns(true);
            _view.TestStatusText.Returns("PASS");
            _view.TestStatusBackColor.Returns(System.Drawing.Color.Green);
            _fileService.FileExists(testPath).Returns(true);
            _folderBrowser.ShowDialog().Returns(false);

            _view.OnSelectTestFolder += Raise.EventWith(null, EventArgs.Empty);
            _view.DidNotReceive().TestPath=Arg.Any<string>();

            Assert.True(_view.ViewOutputButtonEnabled);
            Assert.True(_view.ViewReportButtonEnabled);
            Assert.Equal("PASS", _view.TestStatusText);
            Assert.Equal(System.Drawing.Color.Green, _view.TestStatusBackColor);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(@"c:\temp\out")]
        public void BadgerGuiRunner_ClickOutputBrowse_BrowsesCurrentOutputFolder(string path)
        {
            _view.OutputPath.Returns(path);
            _view.ViewOutputButtonEnabled.Returns(true);
            _view.ViewReportButtonEnabled.Returns(true);
            _folderBrowser.ShowDialog().Returns(true);

            // when no path is specified, use the current directory
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.CurrentDirectory;
            }

            _view.OnSelectOutputFolder += Raise.EventWith(null, EventArgs.Empty);

            _folderBrowser.Received(1).ShowDialog();
            _view.Received(1).OutputPath = path;

            Assert.False(_view.ViewReportButtonEnabled);
            Assert.False(_view.ViewOutputButtonEnabled);
        }
        
        [Fact]
        public void BadgerGuiRunner_BrowseOutputFolderCancel_DoesntChangePath()
        {
            string outPath = @"c:\temp\output";
            _view.OutputPath.Returns(outPath);
            _view.ViewOutputButtonEnabled.Returns(true);
            _view.ViewReportButtonEnabled.Returns(true);
            _fileService.FileExists(outPath).Returns(true);
            _folderBrowser.ShowDialog().Returns(false);

            _view.OnSelectOutputFolder += Raise.EventWith(null, EventArgs.Empty);
            _view.DidNotReceive().OutputPath = Arg.Any<string>();

            Assert.True(_view.ViewOutputButtonEnabled);
            Assert.True(_view.ViewReportButtonEnabled);
        }
        
        [Fact]
        public void BadgerGuiRunner_ViewReport_DisplaysTestSummary()
        {
            _view.OutputPath.Returns(@"C:\temp\testOutput");

            _view.OnViewReport += Raise.EventWith(null, EventArgs.Empty);

            _reportView.Received(1).ShowDialog();
            
        }
        
        [Fact]
        public void BadgerGuiRunner_ViewOutput_DisplaysOutputFolder()
        {
            _view.OutputPath.Returns(@"C:\temp\testOutput");

            _view.OnViewOutput += Raise.EventWith(null, EventArgs.Empty);

            _fileService.Received(1).StartProcess(@"C:\temp\testOutput\Results");
        }
        
        [Fact]
        public void BadgetGuiRunner_ClickNewTest_InvokesTestEditor()
        {
            _view.OnNewTest += Raise.EventWith(null, EventArgs.Empty);
            _editView.Received(1).ShowDialog();
        }
        
    }
}
