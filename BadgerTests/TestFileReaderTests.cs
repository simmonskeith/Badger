using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Badger.Runner;
using Badger.Core.Interfaces;
using NSubstitute;

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
              "    user    keith",
              "    password tralfaz",
              "",
              "*** Teardown ***",
              "Logout",
              "",
              "*** Test Steps ***",
              "Do something that takes no inputs",
              "# This is a comment, it will be ignored.",
              "Do something that needs inputs",
              "    Input1    ksimmons ",
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
            Assert.Equal("BadgerTests.dll", testReader.GetLibraryName());
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

            Assert.Null(testReader.GetLibraryName());
        }

        [Fact]
        public void TestFileReaderShouldReadVaribles()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");
            var variables = testReader.GetVariables();

            Assert.NotEmpty(variables);
            Assert.Equal("${Faker.CreateTimeStamp(\"yyyyMMdd\")}", variables["timestamp"]);
            Assert.Equal("Chewbacca", variables["user"]);
            Assert.Equal("", variables["itsBlank"]);
            Assert.Equal("first line second line third line", variables["multiline"]);
        }

        [Fact]
        public void TestFileReader_GetSetup_ReturnsSetupSteps()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var setup = testReader.GetSetup();

            Assert.Equal(2, setup.Count);
            Assert.True(0 == setup[0].Inputs.Count);
            Assert.Equal(2, setup[1].Inputs.Count);
        }

        [Fact]
        public void TestFileReader_GetTeardown_ReturnsTeardownSteps()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var teardown = testReader.GetTeardown();

            Assert.True(1 == teardown.Count);
            Assert.True(0 == teardown[0].Inputs.Count);
        }


        [Fact]
        public void TestFileReader_FileHasStepsNotHavingInputs_StepsShouldBeLoaded()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var steps = testReader.GetTestSteps();

            // steps 1 and 3 do not have inputs
            Assert.True(0 == steps[0].Inputs.Count);
            Assert.True(0 == steps[2].Inputs.Count);
        }

        [Fact]
        public void TestFileReader_ReadsTestStepsWithInputs_ParsesInputParameters()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");

            var steps = testReader.GetTestSteps();

            // steps 2 has inputs
            Assert.Equal("Do something that needs inputs", steps[1].Keyword);
            Assert.Equal(2, steps[1].Inputs.Count);
            Assert.Equal("ksimmons additional for Input1", steps[1].Inputs["Input1"]);
            Assert.Equal("12345", steps[1].Inputs["Input2"]);
            Assert.Equal("Another step with inputs", steps[3].Keyword);
            Assert.Equal(2, steps[3].Inputs.Count);
            Assert.Equal("${timestamp}", steps[3].Inputs["variable"]);
            Assert.Equal("", steps[3].Inputs["BlankValue"]);
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

            Assert.True(0 == steps.Count);
        }

        [Fact]
        public void TestFileReader_LoadsFileWithCustomKeywords_ParsesCustomKeywords()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");
            var keywords = testReader.GetKeywords();
            Assert.Equal(2, keywords.Count);
            Assert.Equal("Start and login", keywords[0].Keyword);
            Assert.Equal("user", keywords[0].Inputs[0]);
            Assert.Equal("password", keywords[0].Inputs[1]);

            Assert.Equal("Start app", keywords.First(k=>k.Keyword == "Start and login").Steps[0].Keyword);
            Assert.True(0 == keywords.First(k => k.Keyword == "Start and login").Steps.First(s=>s.Keyword == "Start app").Inputs.Count);
            Assert.Equal("Login", keywords.First(k => k.Keyword == "Start and login").Steps[1].Keyword);
            Assert.Equal(2, keywords.First(k => k.Keyword == "Start and login").Steps.First(s => s.Keyword == "Login").Inputs.Count);
            Assert.Equal("${user}", keywords[0].Steps[1].Inputs["user"]);
            Assert.Equal("${password}", keywords[0].Steps[1].Inputs["password"]);

            Assert.Equal("Close app", keywords[1].Keyword);
            Assert.Equal(2, keywords.First(k=>k.Keyword == "Close app").Steps.Count);
            Assert.Equal("Exit", keywords.First(k => k.Keyword == "Close app").Steps[1].Keyword);
            Assert.True(0 == keywords.First(k=>k.Keyword == "Close app").Steps.First(s=>s.Keyword == "Exit").Inputs.Count);
        }

        [Fact]
        public void TestFileReader_LoadFileWithDataSets_ShouldReadDataSets()
        {
            TestFileReader testReader = new TestFileReader(fileService);
            testReader.LoadFile("my test");
            var dataSets = testReader.GetDataSets();
            Assert.Equal(2, dataSets.Count);
            Assert.Equal("Numero Uno", dataSets[0].Name);
            Assert.Equal("Hammerhead", dataSets[0].Inputs["a"]);
            Assert.Equal("Mako", dataSets[0].Inputs["b"]);

            Assert.Equal("El Segundo", dataSets[1].Name);
            Assert.Equal("Orange", dataSets[1].Inputs["x"]);
            Assert.Equal("Red", dataSets[1].Inputs["y"]);
            Assert.Equal("Green", dataSets[1].Inputs["z"]);
        }
    }
}
