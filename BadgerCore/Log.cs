using Badger.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Diagnostics;

namespace Badger.Core
{

    internal class TestStepEvent
    {
        public string Timestamp { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Screenshot { get; set; }
    }

    internal class TestCaseResult
    {
        public int TotalPass;
        public int TotalFail;
        public List<TestStepResult> StepResults;

        public TestCaseResult()
        {
            StepResults = new List<TestStepResult>();
            TotalPass = 0;
            TotalFail = 0;
        }

        public void Add(TestStepResult result)
        {
            StepResults.Add(result);
            TotalPass += result.TotalPass;
            TotalFail += result.TotalFail;
        }
    }

    internal class TestStepResult
    {
        public string Name;
        public int TotalPass;
        public int TotalFail;
        public string OverallResult;
        public List<TestStepEvent> StepEvents;

        public TestStepResult()
        {
            StepEvents = new List<TestStepEvent>();
            TotalPass = 0;
            TotalFail = 0;
            OverallResult = "LOG";
        }

        public void Add(TestStepEvent stepEvent)
        {
            StepEvents.Add(stepEvent);
            if (stepEvent.Type == "PASS")
            {
                TotalPass += 1;
                if (OverallResult == "LOG")
                {
                    OverallResult = "PASS";
                }
            }
            else if (stepEvent.Type == "FAIL")
            {
                TotalFail += 1;
                OverallResult = "FAIL";
            }
        }
    }

    public static class Log 
    {
        private static string _logPath;
        private static string _reportPath;
        private static int _screenshotIndex;
        private static string _screenshotPath;
        private static string _testName;
        private static int _totalPass;
        private static int _totalFail;
        private static TestCaseResult _testCaseResult;
        private static TestStepResult _testStepResult;

        public static IFileService _fileService { get;  set; }
        public static int FailCount
        {
            get { return _totalFail; }
            set { _totalFail = value; }
        }

        private static DateTime _startTime;
        private static Stopwatch _stopwatch;
        private static string _timeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        private static string _path;

        public static void Init(IFileService fileService, string path, string testName)
        {
            
            _fileService = fileService;
            _fileService.DeleteFolder(path, true);
            _fileService.CreateFolder(path);
            _testName = testName;

            _path = path;
            _logPath = Path.Combine(path, "log.txt");
            _reportPath = Path.Combine(path, "report.html");
            _screenshotPath = Path.Combine(path, "screenshots");

            FailCount = 0;
            _screenshotIndex = 1;

            _testCaseResult = new TestCaseResult();

            _startTime = DateTime.Now;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            PostToLog($"Starting test '{testName}'");
        }

        public static void Close()
        {
            var result = FailCount == 0 ? "PASS" : "FAIL";
            if (FailCount > 0)
            {
                PostToLog($"Result: {result} [{FailCount}]");
            }
            else
            {
                PostToLog($"Result: {result}");
            }
            Console.WriteLine("\n");
            CreateHtmlReport();
        }

        public static void StartTestStep(string name, Dictionary<string,string> inputs)
        {
            _testStepResult = new TestStepResult();
            _testStepResult.Name = name;
            // list the parameters and values (in html only?)
            Log.Message($"Starting step '{name}'");
        }

        public static void EndTestStep(string name)
        {
            Log.Message($"Ending step '{name}'\n");
            _testCaseResult.Add(_testStepResult);
        }

        public static void Message(string message)
        {
            var result = new TestStepEvent()
            {
                Timestamp = TimeStamp(),
                Type = "LOG",
                Message = message
            };
            _testStepResult.Add(result);
            PostToLog($"[{result.Timestamp}] LOG: {message}");
        }

        public static void Pass(string message)
        {
            var result = new TestStepEvent()
            {
                Timestamp = TimeStamp(),
                Type = "PASS",
                Message = message
            };
            _testStepResult.Add(result);
            PostToLog($"[{result.Timestamp}] PASS: {message}");
        }

        public static void Fail(string message)
        {
            var screenshot = Path.GetFileName(CreateScreenShot());
            var result = new TestStepEvent()
            {
                Timestamp = TimeStamp(),
                Type = "FAIL",
                Message = message,
                Screenshot = screenshot
            };
            _testStepResult.Add(result);

            PostToLog($"[{result.Timestamp}] FAIL: {message} [{screenshot}]");
            FailCount += 1;
        }

        private static string TimeStamp()
        {
            return DateTime.Now.ToString(_timeFormat);
        }

        private static void PostToLog(string message)
        {
            _fileService.WriteConsole(message + Environment.NewLine);
            _fileService.WriteLine(_logPath, message);
        }

        private static string CreateScreenShot()
        {
            string filename = $"screenshot_{_screenshotIndex}.png";
            if (Directory.Exists(_screenshotPath) == false)
            {
                Directory.CreateDirectory(_screenshotPath);
            }
            string screenshotPath = Path.Combine(_screenshotPath, filename);
            CaptureScreenShot(screenshotPath);
            _screenshotIndex += 1;
            return filename;
        }

        /// <summary>
        /// Captures the entire screen as a screenshot
        /// </summary>
        /// <param name="filename"></param>
        private static void CaptureScreenShot(String filename)
        {
            Rectangle bounds = Screen.GetBounds(Point.Empty);
            using (Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height))
            {
                using (Graphics g = Graphics.FromImage(bitmap))
                {
                    g.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                }
                _fileService.SaveImage(filename, bitmap, ImageFormat.Png);
            }
        }

        public static void CreateHtmlReport()
        {
            string reportCss = "report.css";

            _fileService.CopyFile(Path.Combine(Environment.CurrentDirectory, reportCss),
                    Path.Combine(_path, reportCss));

            var doc = new XDocument();
            doc.Add(new XDocumentType("html", null, null, null));
            var htmlElement = new XElement("html");
            doc.Add(htmlElement);

            var header = new XElement("head");
            htmlElement.Add(header);
            header.Add(new XElement("link",
                new XAttribute("rel", "stylesheet"),
                new XAttribute("type", "text/css"),
                new XAttribute("href", reportCss)));
            header.Add(new XElement("Title", "Test Report"));
            var body = new XElement("body");
            htmlElement.Add(body);


            body.Add(new XElement("h2", _testName));
            body.Add(new XElement("hr"));
            var summaryTable = new XElement("table", new XAttribute("id", "summary"));
            body.Add(summaryTable);
            summaryTable.Add(new XElement("tr", new XAttribute("class", "title"), new XElement("td", "Summary")));
            summaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), new XAttribute("width", "50%"), "Start Time:"),
                new XElement("td", new XAttribute("class", "summary_value"), _startTime.ToString(_timeFormat))));
            summaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "End Time:"),
                new XElement("td", new XAttribute("class", "summary_value"), DateTime.Now.ToString(_timeFormat))));
            var ts = _stopwatch.Elapsed;
            summaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "Elapsed Time:"),
                new XElement("td", new XAttribute("class", "summary_value"), String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours,
                    ts.Minutes, ts.Seconds, ts.Milliseconds / 10))));

            body.Add(new XElement("hr"));
            body.Add(new XElement("h2", "Test Steps"));

            var tocTable = new XElement("table", new XAttribute("id", "toc"));
            tocTable.Add(new XElement("tr", new XAttribute("class", "subheader")),
                new XElement("td", "Test Steps"), new XElement("td", "Overall"), new XElement("td", "Pass"), new XElement("td", "Fail"));
            body.Add(tocTable);


            foreach (var step in _testCaseResult.StepResults)
            {

                var guid = Guid.NewGuid();
                tocTable.Add(new XElement("tr",
                    new XElement("td", new XAttribute("class", "test_desc"), new XElement("a", new XAttribute("href", "#" + guid), step.Name)),
                    new XElement("td", new XAttribute("class", "test_" + step.OverallResult.ToLower()), step.OverallResult[0]),
                    new XElement("td", step.TotalPass),
                    new XElement("td", step.TotalFail)));

                body.Add(new XElement("h4", new XElement("a", new XAttribute("name", guid), new XAttribute("style", "font-weight:bold;"), new XAttribute("href", "#toc"), step.Name)));
 
                var testTable = new XElement("table", new XAttribute("id", "tests"));
                body.Add(testTable);
                body.Add(new XElement("br"));

                testTable.Add(new XElement("tr", new XAttribute("class", "int_header"),
                    new XElement("td", new XAttribute("width", "17%"), "Time"),
                    new XElement("td", new XAttribute("width", "8%"), "Result"),
                    new XElement("td", new XAttribute("width", "75%"), "Comment")));

                foreach (var testevent in step.StepEvents)
                {

                    XElement messageTextElement; ;
                    if (testevent.Screenshot != null)
                    {
                        messageTextElement = new XElement("td", new XAttribute("width", "75%"),
                            testevent.Message + " (",
                            new XElement("a", new XAttribute("href", @"screenshots\" + testevent.Screenshot), new XAttribute("target", "_self"), testevent.Screenshot),
                            ")"//, 
                               //new XElement("br"), 
                               //new XElement("img", 
                               //new XAttribute("src", "data:image/png;base64," + Convert.ToBase64String(File.ReadAllBytes(Path.Combine(_path, "screenshots", testevent.Screenshot)))),
                               //new XAttribute("alt", testevent.Screenshot),
                               //new XAttribute("style", "width:300px;height:200px;")));
                            );
                    }
                    else
                    {
                        messageTextElement = new XElement("td", new XAttribute("width", "75%"), testevent.Message);
                    }

                    testTable.Add(new XElement("tr",
                    new XElement("td", new XAttribute("width", "17%"), testevent.Timestamp),
                    new XElement("td", new XAttribute("width", "8%"), new XAttribute("class", "int_" + testevent.Type.ToLower()), testevent.Type),
                    messageTextElement));
                }
            }

            summaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "Total Passed:"), 
                new XElement("td", new XAttribute("class", "summary_key"), _testCaseResult.TotalPass)));
            summaryTable.Add(new XElement("tr",
                new XElement("td", new XAttribute("class", "summary_key"), "Total Failed:"), 
                new XElement("td", new XAttribute("class", "summary_key"), _testCaseResult.TotalFail)));

            _fileService.SaveHtmlDocument(_reportPath, doc);
        }
    }
}
