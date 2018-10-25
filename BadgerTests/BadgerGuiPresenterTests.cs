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
using FluentAssertions;

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
            _view.NewTestMenuItemEnabled.Returns(true);
            _view.EditTestMenuItemEnabled.Returns(true);

            _view.OnFormLoad += Raise.EventWith(null, EventArgs.Empty);

            bool isTestSelected = !String.IsNullOrEmpty(_view.TestPath);
            bool isResourceFileSelected = !string.IsNullOrEmpty(presenter.ResourcePath);
            _view.RunButtonEnabled.Should().Be(isTestSelected);
            _view.EditTestMenuItemEnabled.Should().Be(isTestSelected);
            _view.ViewOutputMenuItemEnabled.Should().BeFalse();
            _view.ViewReportMenuItemEnabled.Should().BeFalse();
            _view.NewTestMenuItemEnabled.Should().BeTrue();
            _view.ResourceFileLabelVisible.Should().Be(isResourceFileSelected);

        }
        
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("c:\\myResourceFile.txt")]
        public void BadgerGuiRunner_DisplaysResourceFile_IfFilePathIsDefined(string filename)
        {
            presenter = InitPresenter(filename);
            var resourcePath = string.IsNullOrEmpty(filename) ? presenter.ResourcePath : filename;
            var expectedText = String.IsNullOrEmpty(resourcePath) ? "" : $"Resource File: {resourcePath}";
            _view.OnFormLoad += Raise.EventWith(null, EventArgs.Empty);

            _view.Received(1).ResourceFileText = expectedText;
            _view.Received(1).ResourceFileLabelVisible = !String.IsNullOrEmpty(expectedText);
            _view.ResourceFileText.Should().Be(expectedText);
        }
        
        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        [InlineData("c:\\myFile.txt", true)]
        [InlineData("c:\\myFile.txt", false)]
        public void BadgerGuiRunner_Selects_ResourceFile(string originalPath, bool cancel)
        {
            var initialPath = string.IsNullOrEmpty(originalPath) ? presenter.ResourcePath : originalPath;
            presenter = InitPresenter(originalPath);
            var resourcePath = cancel ? initialPath : "C:\\newFile.txt";
            var expectedText = String.IsNullOrEmpty(resourcePath) ? "" : $"Resource File: {resourcePath}";
            _fileService.FileExists(Arg.Any<string>()).Returns(true);
            _fileBrowser.ShowDialog().Returns(!cancel);
            _fileBrowser.FileName.Returns("C:\\newFile.txt");
            _view.OnFormLoad += Raise.EventWith(null, EventArgs.Empty);
            _view.ClearReceivedCalls();

            _view.OnSelectResourceFile += Raise.EventWith(null, EventArgs.Empty);

            _view.Received(cancel ? 0 : 1).ResourceFileText = expectedText;
            expectedText.Should().Be(_view.ResourceFileText);

        }
        
        [Theory]
        [InlineData("", false)]
        [InlineData(@"c:\temp\tests\mytest.txt", true)]
        [InlineData(@"c:\temp\tests", false)]
        public void BadgerGuiRunner_ClickTestBrowse_BrowsesTestFile(string initialPath, bool isFile)
        {
            string newPath = @"c:\temp\tests\newtest.txt";
            _view.TestPath.Returns(initialPath);
            _view.ViewOutputMenuItemEnabled.Returns(true);
            _view.ViewReportMenuItemEnabled.Returns(true);
            _view.EditTestMenuItemEnabled.Returns(false);
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

            _view.ViewReportMenuItemEnabled.Should().BeFalse();
            _view.ViewOutputMenuItemEnabled.Should().BeFalse();
            _view.TestStatusText.Should().Be("No Results");
            _view.EditTestMenuItemEnabled.Should().BeTrue();
            _view.TestStatusBackColor.Should().Be(System.Drawing.Color.SkyBlue);
        }
        
        [Fact]
        public void BadgerGuiRunner_BrowseTestFileCancel_DoesntChangePath()
        {
            string testPath = @"c:\temp\tests\mytest.txt";
            _view.TestPath.Returns(testPath);
            _view.ViewOutputMenuItemEnabled.Returns(true);
            _view.ViewReportMenuItemEnabled.Returns(true);
            _view.TestStatusText.Returns("PASS");
            _view.TestStatusBackColor.Returns(System.Drawing.Color.Green);
            _fileService.FileExists(testPath).Returns(true);
            _folderBrowser.ShowDialog().Returns(false);

            _view.OnSelectTestFile += Raise.EventWith(null, EventArgs.Empty);
            _view.DidNotReceive().TestPath = Arg.Any<string>();

            _view.ViewOutputMenuItemEnabled.Should().BeTrue();
            _view.ViewReportMenuItemEnabled.Should().BeTrue();
            _view.TestStatusText.Should().Be("PASS");
            _view.TestStatusBackColor.Should().Be(System.Drawing.Color.Green);
        }
        
        [Theory]
        [InlineData("", false)]
        [InlineData(@"c:\temp\tests\mytest.txt", true)]
        [InlineData(@"c:\temp\tests", false)]
        public void BadgerGuiRunner_ClickBrowseTestFolder_BrowsesCurrentTestFolder(string path, bool isFile)
        {
            _view.TestPath.Returns(path);
            _view.ViewOutputMenuItemEnabled.Returns(true);
            _view.ViewReportMenuItemEnabled.Returns(true);
            _view.TestStatusText.Returns("PASS");
            _view.EditTestMenuItemEnabled.Returns(isFile);
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

            _view.ViewReportMenuItemEnabled.Should().BeFalse();
            _view.ViewOutputMenuItemEnabled.Should().BeFalse();
            _view.EditTestMenuItemEnabled.Should().BeFalse();
            _view.TestStatusText.Should().Be("No Results");
            _view.TestStatusBackColor.Should().Be(System.Drawing.Color.SkyBlue);

        }
        
        [Fact]
        public void BadgerGuiRunner_BrowseTestFolderCancel_DoesntChangePath()
        {
            string testPath = @"c:\temp\tests";
            _view.TestPath.Returns(testPath);
            _view.ViewOutputMenuItemEnabled.Returns(true);
            _view.ViewReportMenuItemEnabled.Returns(true);
            _view.TestStatusText.Returns("PASS");
            _view.TestStatusBackColor.Returns(System.Drawing.Color.Green);
            _fileService.FileExists(testPath).Returns(true);
            _folderBrowser.ShowDialog().Returns(false);

            _view.OnSelectTestFolder += Raise.EventWith(null, EventArgs.Empty);
            _view.DidNotReceive().TestPath=Arg.Any<string>();

            _view.ViewOutputMenuItemEnabled.Should().BeTrue();
            _view.ViewReportMenuItemEnabled.Should().BeTrue();
            _view.TestStatusText.Should().Be("PASS");
            _view.TestStatusBackColor.Should().Be(System.Drawing.Color.Green);
        }
        
        [Theory]
        [InlineData("")]
        [InlineData(@"c:\temp\out")]
        public void BadgerGuiRunner_ClickOutputBrowse_BrowsesCurrentOutputFolder(string path)
        {
            _view.OutputPath.Returns(path);
            _view.ViewOutputMenuItemEnabled.Returns(true);
            _view.ViewReportMenuItemEnabled.Returns(true);
            _folderBrowser.ShowDialog().Returns(true);

            // when no path is specified, use the current directory
            if (string.IsNullOrEmpty(path))
            {
                path = Environment.CurrentDirectory;
            }

            _view.OnSelectOutputFolder += Raise.EventWith(null, EventArgs.Empty);

            _folderBrowser.Received(1).ShowDialog();
            _view.Received(1).OutputPath = path;

            _view.ViewReportMenuItemEnabled.Should().BeFalse();
            _view.ViewOutputMenuItemEnabled.Should().BeFalse();
        }
        
        [Fact]
        public void BadgerGuiRunner_BrowseOutputFolderCancel_DoesntChangePath()
        {
            string outPath = @"c:\temp\output";
            _view.OutputPath.Returns(outPath);
            _view.ViewOutputMenuItemEnabled.Returns(true);
            _view.ViewReportMenuItemEnabled.Returns(true);
            _fileService.FileExists(outPath).Returns(true);
            _folderBrowser.ShowDialog().Returns(false);

            _view.OnSelectOutputFolder += Raise.EventWith(null, EventArgs.Empty);
            _view.DidNotReceive().OutputPath = Arg.Any<string>();

            _view.ViewOutputMenuItemEnabled.Should().BeTrue();
            _view.ViewReportMenuItemEnabled.Should().BeTrue();
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
