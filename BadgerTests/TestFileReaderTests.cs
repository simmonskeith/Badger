using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Badger.Runner;
using Badger.Core.Interfaces;
using NSubstitute;
using FluentAssertions;

namespace Badger.Tests
{
    [Trait("Category", "TestFileReader")]
    public class TestFileReaderTests
    {
        IFileService fileService;

        public TestFileReaderTests()
        {
            fileService = Substitute.For<IFileService>();
            fileService.GetLines("my test").Returns(new List<string>()
            { "*** Settings ***",
              "Library    BadgerTests.dll",
              "",
              "*** Variables ***",
              "timestamp    ${Faker.CreateTimeStamp(\"yyyyMMdd\")}",
              "user    Chewbacca",
              "multiline    first line",
              "...  second line",
              "...  third line",
              "itsBlank    ",
              "",
              "*** Keywords ***",
              "Start and login    user    password",
              "    Start app",
              "    Login",
              "        user    ${user}",
              "        password    ${password}",
              "",
              "Close app",
              "    Logout",
              "    Exit",
              "",
              "*** Data Sets ***",
              "",
              "Numero Uno",
              "    a    Hammerhead",
              "    b    Mako",
              "El Segundo",
              "    x    Orange",
              "    y    Red",
              "    z    Green",
              "*** Setup ***",
              "Start application",
              "Login",
              "    user    Frodo",
              "    password tralfaz",
              "",
              "*** Teardown ***",
              "Logout",
              "",
              "*** Test Steps ***",
              "Do something that takes no inputs",
              "# This is a comment, it will be ignored.",
              "Do something that needs inputs",
              "    Input1    walla walla ",
              "... additional for Input1",
              "    Input2    12345",
              "Do one more thing",
              "Another step with inputs",
              "    variable    ${timestamp}",
              "    BlankValue    "
            });
            
        }

        [Fact]
        public void TestFileReader_LoadsFileWithLibraryName_ReturnsLibraryName()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");
            testReader.GetLibraryName().Should().Be("BadgerTests.dll");
        }

        [Fact]
        public void TestFileReader_LoadsFileWithoutLibraryName_ReturnsNullLibraryName()
        {
            fileService.GetLines("my test").Returns(new List<string>()
            { "*** Settings ***",
              "Libary  BadgerTests.dll"
            });
            TestFileReader testReader = new TestFileReader(fileService);

            testReader.LoadFile("my test");

            testReader.GetLibraryName().Should().BeNull();
        }

        [Fact]
        public void TestFileReaderShouldReadVaribles()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");
            var variables = testReader.GetVariables();

            variables.Should().NotBeEmpty();
            variables["timestamp"].Should().Be("${Faker.CreateTimeStamp(\"yyyyMMdd\")}");
            variables["user"].Should().Be("Chewbacca");
            variables["itsBlank"].Should().BeEmpty();
            variables["multiline"].Should().Be("first line second line third line");
        }

        [Fact]
        public void TestFileReader_GetSetup_ReturnsSetupSteps()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var setup = testReader.GetSetup();

            setup.Should().HaveCount(2);
            setup[0].Inputs.Should().HaveCount(0);
            setup[1].Inputs.Should().HaveCount(2);
        }

        [Fact]
        public void TestFileReader_GetTeardown_ReturnsTeardownSteps()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var teardown = testReader.GetTeardown();

            teardown.Should().HaveCount(1);
            teardown[0].Inputs.Should().HaveCount(0);
        }

        [Fact]
        public void TestFileReader_FileHasStepsNotHavingInputs_StepsShouldBeLoaded()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var steps = testReader.GetTestSteps();

            // steps 1 and 3 do not have inputs
            steps[0].Inputs.Should().HaveCount(0);
            steps[2].Inputs.Should().HaveCount(0);
        }

        [Fact]
        public void TestFileReader_ReadsTestStepsWithInputs_ParsesInputParameters()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var steps = testReader.GetTestSteps();

            // steps 2 has inputs
            steps[1].Keyword.Should().Be("Do something that needs inputs");
            steps[1].Inputs.Should().HaveCount(2);
            steps[1].Inputs["Input1"].Should().Be("walla walla additional for Input1");
            steps[1].Inputs["Input2"].Should().Be("12345");
            steps[3].Keyword.Should().Be("Another step with inputs");
            steps[3].Inputs.Should().HaveCount(2);
            steps[3].Inputs["variable"].Should().Be("${timestamp}");
            steps[3].Inputs["BlankValue"].Should().BeEmpty();
        }

        [Fact]
        public void TestFileReader_LoadsFileWithoutTestCase_ReturnsZeroSteps()
        {
            fileService.GetLines(Arg.Any<string>()).Returns(new List<string>()
            { "*** Settings ***",
              "Library  BadgerTests.dll",
              "",
              "*** Variables ***",
              "timestamp  ${Faker.CreateTimeStamp(\"yyyyMMdd\")}",
              "user  Chewbacca" });
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var steps = testReader.GetTestSteps();

            steps.Should().HaveCount(0);
        }

        [Fact]
        public void TestFileReader_LoadsFileWithCustomKeywords_ParsesCustomKeywords()
        {
            TestFileReader testReader = new TestFileReader(fileService);

            testReader.LoadFile("my test");
            var keywords = testReader.GetKeywords();

            keywords.Should().HaveCount(2);
            keywords[0].Keyword.Should().Be("Start and login");
            keywords[0].Inputs[0].Should().Be("user");
            keywords[0].Inputs[1].Should().Be("password");

            keywords.First(k => k.Keyword == "Start and login").Steps[0].Keyword.Should().Be("Start app");
            keywords.First(k => k.Keyword == "Start and login")
                .Steps.First(s => s.Keyword == "Start app")
                .Inputs.Should().HaveCount(0);
            keywords.First(k => k.Keyword == "Start and login").Steps[1].Keyword.Should().Be("Login");
            keywords.First(k => k.Keyword == "Start and login")
                .Steps.First(s => s.Keyword == "Login")
                .Inputs.Should().HaveCount(2);
            keywords[0].Steps[1].Inputs["user"].Should().Be("${user}");
            keywords[0].Steps[1].Inputs["password"].Should().Be("${password}");

            keywords[1].Keyword.Should().Be("Close app");
            keywords.First(k => k.Keyword == "Close app").Steps.Should().HaveCount(2);
            keywords.First(k => k.Keyword == "Close app").Steps[1].Keyword.Should().Be("Exit");
            keywords.First(k => k.Keyword == "Close app")
                .Steps.First(s => s.Keyword == "Exit")
                .Inputs.Should().HaveCount(0);
        }

        [Fact]
        public void TestFileReader_LoadFileWithDataSets_ShouldReadDataSets()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");
            var dataSets = testReader.GetDataSets();
            dataSets.Should().HaveCount(2);
            dataSets[0].Name.Should().Be("Numero Uno");
            dataSets[0].Inputs["a"].Should().Be("Hammerhead");
            dataSets[0].Inputs["b"].Should().Be("Mako");
            dataSets[1].Name.Should().Be("El Segundo");
            dataSets[1].Inputs["x"].Should().Be("Orange");
            dataSets[1].Inputs["y"].Should().Be("Red");
            dataSets[1].Inputs["z"].Should().Be("Green");
        }
    }
}
