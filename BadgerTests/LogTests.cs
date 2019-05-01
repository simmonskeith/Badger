using Badger.Core;
using Badger.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Xunit;
using NSubstitute;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Badger.Tests
{
    [Trait("Category", "Log")]
    public class LogTests
    {
        IFileService _fileService;
        string _logpath = @"c:\temp\logtest";

        string timestampRegex = @"\[20\d\d-[0-1]\d-[0-3]\d [0-2]\d:[0-5]\d:[0-5]\d\.\d{0,3}\]";

        public LogTests()
        {
            _fileService = Substitute.For<IFileService>();
            _fileService.CreateFolder(Arg.Any<string>());
            Log.Init(_fileService, _logpath, "My Mock Test");
        }

        [Fact]
        public void Log_Message_WritesMessageToFile()
        {
            var expectedString = new Regex(timestampRegex + " LOG: Hello World!");
            Log.StartTestStep(Arg.Any<string>(), new Dictionary<string, string>());

            Log.Message("Hello World!");

            _fileService.Received(1).WriteLine(Path.Combine(_logpath, "log.txt"), Arg.Is<string>(s=>expectedString.IsMatch(s)));
            _fileService.Received(1).WriteConsole(Arg.Is<string>(s => expectedString.IsMatch(s)));
            Log.FailCount.Should().Be(0);
        }

        
        [Fact]
        public void Log_Pass_WritesPassMessageToFile()
        {
            var expectedString = new Regex(timestampRegex + " PASS: This step passed.");
            Log.StartTestStep("some step", new Dictionary<string, string>());

            Log.Pass("This step passed.");

            _fileService.Received(1).WriteLine(Path.Combine(_logpath, "log.txt"), Arg.Is<string>(s=>expectedString.IsMatch(s)));
            _fileService.Received(1).WriteConsole(Arg.Is<string>(s=>expectedString.IsMatch(s)));
            Log.FailCount.Should().Be(0);
        }
        
        [Fact]
        public void Log_Fail_WritesFailMessage()
        {
            var expectedString = new Regex(timestampRegex + " FAIL: Failed step \\[screenshot_1.png\\]");
            var func = Substitute.For<Action<string>>();
            Log.SetScreenShotDelegate(func);
            Log.StartTestStep("some step", new Dictionary<string, string>());

            Log.Fail("Failed step");

            _fileService.Received(1).WriteLine(Path.Combine(_logpath, "log.txt"), Arg.Is<string>(s => expectedString.IsMatch(s)));
            func.Received(1);
            _fileService.Received(1).WriteConsole(Arg.Is<string>(s => expectedString.IsMatch(s)));
        }

        [Fact]
        public void Log_Close_DisplaysFailCount()
        {
            Log.StartTestStep("some step", new Dictionary<string, string>());
            Log.Fail("Failed step");

            Log.Close();

            _fileService.Received(1).WriteLine(Path.Combine(_logpath, "log.txt"), "Result: FAIL [1]");
        }

        [Fact]
        public void Log_Fail_IncrementsFailCounter()
        {
            Log.StartTestStep("some step", new Dictionary<string, string>());

            Log.Fail("Failed step");

            Log.FailCount.Should().Be(1);
        }

        [Fact]
        public void Log_SetScreenshotDelegate_SetsDelegateFunction()
        {
            var func = Substitute.For<Action<string>>();
            Log.SetScreenShotDelegate(func);
            Log.StartTestStep("some step", new Dictionary<string, string>());
            Log.Fail("Failed step");
            func.Received(1);
        }

        [Fact]
        public void CaptureScreenshot_OnCapture_GeneratesScreenshot()
        {
            ScreenCapture.CaptureScreenShot(_fileService, "screenshot_1.png");
            _fileService.Received(1).SaveImage("screenshot_1.png", Arg.Any<System.Drawing.Bitmap>(),
                Arg.Any<System.Drawing.Imaging.ImageFormat>());
        }

    }
}
