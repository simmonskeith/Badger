using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Badger.Core;
using Badger.Runner;
using NSubstitute;
using Badger.Core.Interfaces;
using System.Text.RegularExpressions;
using FluentAssertions;

namespace Badger.Tests
{
    [Trait("Category", "Test Service")]
    public class TestServiceTests
    {
        IFileService _fileReader;

        public TestServiceTests()
        {
            _fileReader = Substitute.For<IFileService>();
            _fileReader.GetLines(Arg.Any<string>()).Returns(new List<string>()
            { "*** Settings ***",
              "Library    " + Assembly.GetAssembly(typeof(TestServiceTests)).GetName().Name,
              "",
              "*** Variables ***"
            });
        }

        [Fact]
        public void TestService_WhenTestCaseIsLoaded_LoadsAssembly()
        {
            var service = GetTestService();
            service.Should().NotBeNull();
        }

        [Fact]
        public void TestService_GivenATestFileWithASpecifiedLibrary_RetrievesAvailableTestSteps()
        {
            var service = new TestService(_fileReader);
            service.Init("", null);
            TestService.GetStepMethods().Should().HaveCount(6);
        }

        [Fact]
        public void TestService_GivenAKeyword_RetrievesSpecifiedStepMethod()
        {
            var service = new TestService(_fileReader);
            service.Init("", null);
            var method = TestService.GetStepMethod("This is step #2");
            method.Should().NotBeNull();
        }

        [Fact]
        public void TestService_GivenAnInlineParameterKeyword_RetrievesSpecifiedStepMethod()
        {
            var service = new TestService(_fileReader);
            service.Init("", null);
            var method = TestService.GetStepMethod("An inline \"myValue\" test step");
            method.Should().NotBeNull();
        }

        [Fact]
        public void TestService_GivenAnInlineKeyword_RunsAssociatedStepMethod()
        {
            var mockWriter = Substitute.For<IFileService>();
            mockWriter.CreateFolder(Arg.Any<string>());
            mockWriter.WriteLine(Arg.Any<string>(), Arg.Any<string>());

            var service = GetTestService();
            Log.Init(mockWriter, @"c:\temp", "Some Test");
            Log.StartTestStep("my test step", new Dictionary<string, string>());
            service.CallTestStepMethod(new TestStep()
            {
                Keyword = "An inline \"Some value 123 @#$%^!&\" test step",
                Inputs = new Dictionary<string, string>() { }
            });
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x =>
                new Regex($@"^\[.*\] LOG: Got 'Some value 123 @#\$%\^!&'$").IsMatch(x)));
            Log.FailCount.Should().Be(0);
        }

        [Fact]
        public void TestService_GivenAKeyword_RunsAssociatedStepMethod()
        {
            var mockWriter = Substitute.For<IFileService>();
            mockWriter.CreateFolder(Arg.Any<string>());
            mockWriter.WriteLine(Arg.Any<string>(), Arg.Any<string>());

            var service = GetTestService();
            Log.Init(mockWriter, @"c:\temp", "Some Test");
            Log.StartTestStep("my test step", new Dictionary<string, string>());
            service.CallTestStepMethod(new TestStep() { Keyword = "I am the first step" });
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x=>
                new Regex($"^\\[.*\\] LOG: This is the first step$").IsMatch(x)));
            Log.FailCount.Should().Be(0);
        }

        [Fact]
        public void TestServiceShouldHandleStepsWithDictionaryInputs()
        {
            var mockWriter = Substitute.For<IFileService>();
            mockWriter.CreateFolder(Arg.Any<string>());
            mockWriter.WriteLine(Arg.Any<string>(), Arg.Any<string>());

            var service = new TestService(_fileReader);
            service.Init("", null);
            Log.Init(mockWriter, @"c:\temp", "Some Test");
            Log.StartTestStep("my test step", new Dictionary<string, string>());
            service.CallTestStepMethod(new TestStep() { Keyword = "Step with dictionary",
                Inputs = new Dictionary<string, string>() { { "MyValue", "AC" } } });
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x=>
                new Regex($"^\\[.*\\] LOG: First value is AC$").IsMatch(x)));
            Log.FailCount.Should().Be(0);
        }

        [Fact]
        public void TestService_WhenTestCaseWithCustomStepsIsRun_RunsCustomSteps()
        {
            var mockWriter = Substitute.For<IFileService>();
            mockWriter.CreateFolder(Arg.Any<string>());
            mockWriter.WriteLine(Arg.Any<string>(), Arg.Any<string>());

            _fileReader.GetLines(Arg.Any<string>()).Returns(new List<string>()
            { "*** Settings ***",
              "Library    " + Assembly.GetAssembly(typeof(TestServiceTests)).GetName().Name,
              "",
              "*** Variables ***",
              "",
              "*** Keywords ***",
              "Custom Keyword    arg1    arg2",
              "    I am the first step",
              "    A step with inputs",
              "        a    ${arg2}",
              "        b    ${arg1}",
              "",
              "*** Test Steps ***",
              "Another step",
              "Custom Keyword",
              "    arg1    Cat",
              "    arg2    Mouse"
            });

            var service = new TestService(_fileReader);
            service.Init("", null);
            Log.Init(mockWriter, @"c:\temp", "Some Test");
            service.RunTestSteps();
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x=> 
                new Regex($"^\\[.*\\] FAIL: I failed").IsMatch(x)));
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x=>
                new Regex($"^\\[.*\\] LOG: This is the first step").IsMatch(x)));
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x=>
                new Regex($"^\\[.*\\] LOG: Got 'Mouse' and 'Cat'").IsMatch(x)));

        }

        [Fact]
        public void TestService_WhenTestCaseWithDataSetsIsLoaded_MergesDataSetsWithStepInputs()
        {
            var mockWriter = Substitute.For<IFileService>();
            mockWriter.CreateFolder(Arg.Any<string>());
            mockWriter.WriteLine(Arg.Any<string>(), Arg.Any<string>());

            _fileReader.GetLines(Arg.Any<string>()).Returns(new List<string>()
            { "*** Settings ***",
              "Library    " + Assembly.GetAssembly(typeof(TestServiceTests)).GetName().Name,
              "",
              "*** Variables ***",
              "",
              "*** Data Sets ***",
              "MyDataSet",
              "    a    Car",
              "",
              "*** Test Steps ***",
              "A step with inputs",
              "    DataSet1    MyDataSet",
              "    b    Truck"
            });

            var service = new TestService(_fileReader);
            service.Init("", null);
            Log.Init(mockWriter, @"c:\temp", "Some Test");
            service.RunTestSteps();
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x=>
                new Regex($"^\\[.*\\] LOG: Got 'Car' and 'Truck'").IsMatch(x)));
        }

        [Fact]
        public void TestService_WhenTestCaseWithDataSetsAndCustomStepsLoaded_MergesDataSetWithCustomStepInputs()
        {
            var mockWriter = Substitute.For<IFileService>();
            mockWriter.CreateFolder(Arg.Any<string>());
            mockWriter.WriteLine(Arg.Any<string>(), Arg.Any<string>());

            _fileReader.GetLines(Arg.Any<string>()).Returns(new List<string>()
            { "*** Settings ***",
              "Library    " + Assembly.GetAssembly(typeof(TestServiceTests)).GetName().Name,
              "",
              "*** Variables ***",
              "",
              "*** Data Sets ***",
              "MyDataSet",
              "    a    Car",
              "    b    Bus",
              "",
              "*** Keywords ***",
              "Custom Keyword    arg1",
              "    A step with inputs",
              "        DataSet1    MyDataSet",
              "        b    ${arg1}",
              "",
              "*** Test Steps ***",
              "Another step",
              "Custom Keyword",
              "    arg1    Truck"
            });

            var service = new TestService(_fileReader);
            service.Init("", null);
            Log.Init(mockWriter, @"c:\temp", "Some Test");
            service.RunTestSteps();
            mockWriter.Received(1).WriteLine(@"c:\temp\log.txt", Arg.Is<string>(x=>
                new Regex($"^\\[.*\\] LOG: Got 'Car' and 'Truck'").IsMatch(x)));
        }

        private TestService GetTestService()
        {

            var testManager = new TestService(_fileReader);
            testManager.Init(Arg.Any<string>(), null);
            return testManager;
        }


    }

    [Steps("Sample test steps")]
    public class SampleTests
    {
        [Step("I am the first step")]
        public void FirstStep()
        {
            Log.Message("This is the first step");
        }

        [Step("This is step #2")]
        public void SecondStep()
        {
            Log.Pass("I passed!");
        }

        [Step("Another step")]
        public void ThirdStep()
        {
            Log.Fail("I failed.");
        }

        [Step("Step with dictionary")]
        public void StepWithDictionary(Dictionary<string,string> values)
        {
            Log.Message($"First value is {values[values.Keys.First()]}");
        }

        [Step("A step with inputs")]
        public void StepWithInputs(string a, string b)
        {
            Log.Message($"Got '{a}' and '{b}'");
        }

        [Step("An inline \"parameter\" test step")]
        public void StepWithInlineParameter(string parameter)
        {
            Log.Message($"Got '{parameter}'");
        }
    }
}
