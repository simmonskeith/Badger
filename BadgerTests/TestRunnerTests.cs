using Badger.Runner;
using Badger.Runner.Interfaces;
using Badger.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.Xml.Linq;
using NSubstitute;

namespace Badger.Tests
{
    [Trait("Category", "TestRunner")]
    public class TestRunnerTests
    {
        ITestService _testManager;
        IFileService _fileService;

        public TestRunnerTests()
        {
            _testManager = Substitute.For<ITestService>();
            _testManager.RunSetup().Returns(true);
            _testManager.RunTearDown().Returns(true);
            _fileService = Substitute.For<IFileService>();
            _fileService.LoadHtmlFile(Arg.Any<string>()).Returns(MakeSummaryHtml());
            _fileService.IsDirectory(Arg.Any<string>()).Returns(false);
        }

        [Fact]
        public void TestRunner_WhenTestIsRun_CreatesSummaryReport()
        {
            _fileService.IsDirectory(Arg.Any<string>()).Returns(true);
            _fileService.GetFiles(Arg.Any<string>(), "*.txt", System.IO.SearchOption.AllDirectories)
                .Returns(new List<string>() { "Test A.txt" });
            _testManager.RunTestSteps().Returns(true);
            var runner = new TestRunner(_testManager, _fileService);

            var result = runner.RunTests(@"c:\some directory\test folder", @"c:\results", null);

            // should save before test, after step, and then in closing the log.
            _fileService.Received(3).SaveHtmlDocument(@"c:\results\Results\summary.html", Arg.Any<XDocument>());
            _fileService.Received(1).CopyFile(System.IO.Path.Combine(Environment.CurrentDirectory, "summary.css"), @"c:\results\Results\summary.css");
        }
        
        [Fact]
        public void TestRunner_WhenSingleTestFileIsSelectedAndRunCalled_RunsTestCase()
        {
            _fileService.IsDirectory(Arg.Any<string>()).Returns(true);
            _fileService.GetFiles(Arg.Any<string>(), "*.txt", System.IO.SearchOption.AllDirectories)
                .Returns(new List<string>() { "Test A.txt" });
            _testManager.RunTestSteps().Returns(true);
            _testManager.Init("Test A.txt", null).Returns(true);
            var runner = new TestRunner(_testManager, _fileService);

            var result = runner.RunTests(@"c:\some directory\test folder", @"c:\results", null);

            _fileService.Received(1).GetFiles(@"c:\some directory\test folder", "*.txt", System.IO.SearchOption.AllDirectories);
            _testManager.Received(1).RunTestSteps();
        }
        
        [Fact]
        public void TestRunner_WhenTestDirectorySelectedAndRunCalled_RunsMultipleTestFiles()
        {
            _fileService.IsDirectory(Arg.Any<string>()).Returns(true);
            _fileService.GetFiles(Arg.Any<string>(), "*.txt", System.IO.SearchOption.AllDirectories)
                .Returns(new List<string>() { "Test A.txt", "Test B.txt" });
            _testManager.Init(Arg.Any<string>(), Arg.Any<string>()).Returns(true);
            _testManager.RunTestSteps().Returns(true);
            var runner = new TestRunner(_testManager, _fileService);

            var result = runner.RunTests(@"c:\some directory\test folder", @"c:\results", null);

            _fileService.Received(1).IsDirectory(@"c:\some directory\test folder");
            _fileService.Received(1).CreateFolder(@"c:\results\Results\Test A");
            _fileService.Received(1).CreateFolder(@"c:\results\Results\Test B");
            _fileService.Received(4).SaveHtmlDocument(@"c:\results\Results\summary.html", Arg.Any<XDocument>());
            Assert.True(result);
        }
        
        [Fact]
        public void TestRunner_WhenATestWithoutStepsIsSelected_DoesNotRunTheTest()
        {
            _fileService.IsDirectory(Arg.Any<string>()).Returns(true);
            _fileService.GetFiles(Arg.Any<string>(), "*.txt", System.IO.SearchOption.AllDirectories)
                .Returns(new List<string>() { "Test A.txt" });
            _testManager.RunTestSteps().Returns(true);
            _testManager.Init("Test A.txt", null).Returns(false);
            var runner = new TestRunner(_testManager, _fileService);

            var result = runner.RunTests(@"c:\some directory\test folder", @"c:\results", null);

            _testManager.DidNotReceive().RunTestSteps();
        }
        
        [Fact]
        public void TestRunner_WhenATestInAGroupFails_ReportsFailForTheTestRun()
        {
            _fileService.IsDirectory(Arg.Any<string>()).Returns(true);
            _fileService.GetFiles(Arg.Any<string>(), "*.txt", System.IO.SearchOption.AllDirectories)
                .Returns(new List<string>() { "Test A.txt", "Test B.txt" });
            var runner = new TestRunner(_testManager, _fileService);

            var result = runner.RunTests(@"c:\some directory\test folder", @"c:\results", null);

            _fileService.Received(1).CreateFolder(@"c:\results\Results\Test A");
            _fileService.Received(1).CreateFolder(@"c:\results\Results\Test B");
            Assert.False(result);
        }
        
        [Fact]
        public void TestRunner_WhenPathWithoutTestsIsSelected_DoesNotRunTests()
        {
            _fileService.IsDirectory(Arg.Any<string>()).Returns(true);
            _fileService.GetFiles(Arg.Any<string>(), "*.txt", System.IO.SearchOption.AllDirectories)
                .Returns(new List<string>());
            var runner = new TestRunner(_testManager, _fileService);

            var result = runner.RunTests(@"c:\some directory\test folder", @"c:\results", null);
 
            //_testManager.Verify(t => t.RunTest(It.IsAny<string>()), Times.Never);
            _fileService.DidNotReceive().SaveHtmlDocument(Arg.Any<string>(), Arg.Any<XDocument>());
            Assert.True(result);
        }
        
        private XDocument MakeSummaryHtml()
        {
            var doc = new XDocument();
            doc.Add(new XDocumentType("html", null, null, null));
            var htmlElement = new XElement("html");
            doc.Add(htmlElement);

            var header = new XElement("head");
            htmlElement.Add(header);

            header.Add(new XElement("Title", "Test Summary"));
            var body = new XElement("body");
            htmlElement.Add(body);

            body.Add(new XElement("h3", "Summary"));
            var xSummaryTable = new XElement("table", new XAttribute("id", "summary"));
            xSummaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "Start Time:"),
                new XElement("td", new XAttribute("class", "summary_value"), DateTime.Now.ToString())));
            xSummaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "End Time:"),
                new XElement("td", new XAttribute("class", "summary_value"), "")));
            xSummaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "Elapsed Time:"),
                new XElement("td", new XAttribute("class", "summary_value"), "")));
            body.Add(xSummaryTable);

            body.Add(new XElement("hr"));
            body.Add(new XElement("h3", "Test Cases"));

            var summaryTable = new XElement("table", new XAttribute("id", "testcases"));
            summaryTable.Add(new XElement("tr"),
                new XElement("th", "Test"), new XElement("th", "Result"));
            body.Add(summaryTable);

            return doc;
        }
    }
}
