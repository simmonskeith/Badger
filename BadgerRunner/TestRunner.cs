using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Badger.Core;
using Badger.Runner.Interfaces;

using System.Xml.Linq;
using System.Xml.XPath;
using System.IO;
using Badger.Core.Interfaces;
using System.Diagnostics;

namespace Badger.Runner
{
    public class TestRunner
    {
        private IFileService _fileService;
        private ITestService _manager;
        private const string SUMMARY_FILENAME = "summary.html";
        private Stopwatch _stopwatch;
        private DateTime _startTime;

        public TestRunner(ITestService manager, IFileService fileService)
        {
            _manager = manager;
            _fileService = fileService;
        }

        private List<string> GetTests(string path)
        {
            if (_fileService.IsDirectory(path))
            {
                return _fileService.GetFiles(path, "*.txt", System.IO.SearchOption.AllDirectories).ToList();
            }
            else
            {
                return new List<string>() { path };
            }
        }

        public bool RunTests(string testpath, string outpath, string resourcePath)
        {
            bool result = true;
            var resultPath = Path.Combine(outpath, "Results");

            var tests = GetTests(testpath);
            if (tests.Count == 0)
            {
                Console.WriteLine("No tests found.");
            }
            else
            {
                try
                {
                    InitSummary(resultPath);

                    foreach (var test in tests)
                    {
                        // setup the logging folder
                        var testName = Path.GetFileNameWithoutExtension(test);

                        Log.Init(_fileService, Path.Combine(resultPath, testName), testName);
                        bool currentResult = RunTest(test, resourcePath);
                        result &= currentResult;
                        Log.Close();

                        AppendSummary(resultPath, testName, currentResult);
                    }

                    CompleteSummary(resultPath);
                }
                catch(Exception e)
                {
                    Console.WriteLine("An error occcured.");
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine("Stack trace: " + e.StackTrace);
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Runs the specified test.  Returns 0 if Passed, 1 if failed.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool RunTest(string path, string resourcePath)
        {
            bool result = true;
            try
            {
                //DataStore.Initialize();

                if (false == _manager.Init(path, resourcePath))
                {
                    return false;
                }

                // 2. If any test setups were specified, run them first
                result &= _manager.RunSetup();

                // run steps if setup succeeded
                if (result)
                {
                    result &= _manager.RunTestSteps();
                }

                // run teardowns
                result &= _manager.RunTearDown();
            }
            catch (Exception e)
            {
                // if a problem occurs with reading the test case, abort the test.
                Console.WriteLine("A test case exception.");
                Console.WriteLine("Test Case: " + path);
                Console.WriteLine("Error: " + e.Message);
                Console.WriteLine("Stack trace: " + e.StackTrace);
                return false;
            }
            return result;
        }

        public void InitSummary(string path)
        {
            var reportPath = Path.Combine(path, SUMMARY_FILENAME);
            _fileService.DeleteFolder(path, true);
            _fileService.CreateFolder(path);

            string reportCss = "summary.css";

            _fileService.CopyFile(Path.Combine(Environment.CurrentDirectory, reportCss),
                    Path.Combine(path, reportCss));

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _startTime = DateTime.Now;


            var doc = new XDocument();
            doc.Add(new XDocumentType("html", null, null, null));
            var htmlElement = new XElement("html");
            doc.Add(htmlElement);

            var header = new XElement("head");
            header.Add(new XElement("link",
               new XAttribute("rel", "stylesheet"),
               new XAttribute("type", "text/css"),
               new XAttribute("href", "summary.css")));
            htmlElement.Add(header);

            header.Add(new XElement("Title", "Test Summary"));
            var body = new XElement("body");
            htmlElement.Add(body);

            body.Add(new XElement("h3", "Summary"));
            var xSummaryTable = new XElement("table", new XAttribute("id", "summary"));
            xSummaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "Start Time:"),
                new XElement("td", new XAttribute("class", "summary_value"), _startTime.ToString())));
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
            summaryTable.Add(new XElement("tr",
                new XElement("th", "Test"), new XElement("th", "Result")));
            body.Add(summaryTable);

            _fileService.SaveHtmlDocument(reportPath, doc);
        }

        public void AppendSummary(string path, string testName, bool result)
        {
            var reportPath = Path.Combine(path, SUMMARY_FILENAME);
            var doc = _fileService.LoadHtmlFile(reportPath);
            var table = doc.XPathSelectElement("//table[@id='testcases']");

            var xRow = new XElement("tr");
            xRow.Add(new XElement("td", new XAttribute("class", "test_desc"), testName));
            xRow.Add(new XElement("td", new XAttribute("class", result ? "test_pass" : "test_fail"),
                new XElement("a", new XAttribute("href", Path.Combine(testName, "report.html")), result ? "PASS":"FAIL")));

            table.Add(xRow);

            _fileService.SaveHtmlDocument(reportPath, doc);
        }

        public void CompleteSummary(string path)
        {
            _stopwatch.Stop();
            string endTime = DateTime.Now.ToString();
            var ts = _stopwatch.Elapsed;
            _stopwatch.Reset();
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            var reportPath = Path.Combine(path, SUMMARY_FILENAME);
            var doc = _fileService.LoadHtmlFile(reportPath);

            var table = doc.XPathSelectElement("//table[@id='summary']");
            table.XPathSelectElement(".//tr[2]/td[2]").Value = endTime;
            table.XPathSelectElement(".//tr[3]/td[2]").Value = elapsedTime;

            _fileService.SaveHtmlDocument(reportPath, doc);
        }
    }
}
