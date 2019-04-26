using System;
using System.Collections.Generic;
using System.Linq;
using Badger.Core.Interfaces;
using Badger.Runner.Interfaces;
using System.Text.RegularExpressions;

namespace Badger.Runner
{

    public class DataSet
    {
        public string Name { get; set; }
        public Dictionary<string,string> Inputs { get; set; }
    }

    public class TestFileReader : ITestFileReader
    {
        List<string> _lines;
        IFileService _fileService;

        public TestFileReader(IFileService fileService)
        {
            _fileService = fileService;
        }

        public void LoadFile(string path)
        {
            _lines = _fileService.GetLines(path);
        }

        public string GetLibraryName()
        {
            var section = GetSettingsSection();
            if (section.Count() == 0)
            {
                return null;
            }
            string libLine = section.First(x => x.Contains("Library    "));
            if (libLine == null)
            {
                return null;
            }
            var result = libLine.Split(new[] { "Library" }, StringSplitOptions.None);
            if (result.Length < 2)
            {
                return null;
            }
            return result[1].Trim();
        }

        private List<string> GetSettingsSection()
        {
            int start = _lines.IndexOf("*** Settings ***");
            int end = _lines.FindIndex(start + 1, x => x.StartsWith("*** "));
            if (start < 0 || end < 0)
            {
                return new List<string>();
            }
            var settingsLines = _lines.GetRange(start, end - start);
            return settingsLines;
        }

        public List<string> GetTags()
        {
            var tags = new List<string>();
            var section = GetSettingsSection();
            if (section.Count() == 0)
            {
                return tags;
            }
            string tagsLine = section.FirstOrDefault(x => x.Contains("Tags"));
            if (tagsLine == null)
            {
                return tags;
            }

            var result = tagsLine.Split(new[] { "Tags" }, StringSplitOptions.None);
            if (result.Length < 2)
            {
                return null;
            }
            tags = result[1].Split(',').ToList().Select(t => t.Trim()).ToList();
            return tags;
        }

        public Dictionary<string, string> GetVariables()
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();
            int start = _lines.IndexOf("*** Variables ***");
            int end = _lines.FindIndex(start + 1, x => x.StartsWith("*** "));
            if (end < 0)
            {
                end = _lines.Count();
            }
            //if (start < 0 || end < 0)
            if (start < 0)
            {
                return variables;
            }
            var section = _lines.GetRange(start, end - start);
            for(int i = 0; i<section.Count; i++)
            {
                if (Regex.IsMatch(section[i], @"^\w.*"))
                {
                    var fields = section[i].Split(new[] { "    " }, StringSplitOptions.None);
                    if (fields.Length < 2)
                    {
                        variables.Add(fields[0].Trim(), "");
                    }
                    else
                    {
                        int n = 1;
                        // check for more input on the next line using the '...' syntax
                        while ((section.Count > i + n) && Regex.IsMatch(section[i + n], "\\.{3}\\s.*"))
                        {
                            fields[1] += section[i + n].Substring(4);
                            n += 1;

                        }
                        variables.Add(fields[0].Trim(), fields[1]);
                    }
                }
                
                
            }
            return variables;
        }

        public List<TestStep> GetSetup()
        {
            int start = _lines.IndexOf("*** Setup ***");
            int end = _lines.FindIndex(start + 1, x => x.StartsWith("*** "));
            return GetSectionSteps(start, end).Select(s => { s.IsSetup = true; return s; }).ToList();
        }

        public List<TestStep> GetTeardown()
        {
            int start = _lines.IndexOf("*** Teardown ***");
            int end = _lines.FindIndex(start + 1, x => x.StartsWith("*** "));
            return GetSectionSteps(start, end).Select(s => { s.IsTeardown = true; return s; }).ToList();
        }


        public List<TestStep> GetTestSteps()
        {
            var steps = new List<TestStep>();
            int start = _lines.IndexOf("*** Test Steps ***");
            int end = _lines.Count - 1;
            if (start < 0 || end < 0)
            {
                return steps;
            }
            start += 1;

            for (int n=start; n <= end; n++)
            {
                if(Regex.IsMatch(_lines[n], @"^\w.*") )
                {
                    steps.Add(new TestStep() { Keyword = _lines[n].Trim(), Inputs = GetInputs(_lines, n+1) });
                }
            }
            return steps;
        }

        public List<CustomKeyword> GetKeywords()
        {
            List<CustomKeyword> keywords = new List<CustomKeyword>();
            int start = _lines.IndexOf("*** Keywords ***");
            int end = _lines.FindIndex(start + 1, x => x.StartsWith("*** "));
            if (start < 0 || end < 0)
            {
                return keywords;
            }
            var section = _lines.GetRange(start, end - start);
            for (int i = 0; i < section.Count; i++)
            {
                if (Regex.IsMatch(section[i], @"^\w.*"))
                {
                    var keyword = new CustomKeyword();
                    var fields = section[i].Split(new[] { "    " }, StringSplitOptions.None);
                    keyword.Keyword = fields[0];
                    keyword.Inputs = fields.ToList();
                    keyword.Inputs.RemoveAt(0);
                    keyword.Steps = GetKeywordSubsteps(start+i, start+section.Count);
                    keywords.Add(keyword);
                }
            }
            return keywords;
        }

        public List<DataSet> GetDataSets()
        {
            var dataSets = new List<DataSet>();

            int start = _lines.IndexOf("*** Data Sets ***");
            int end = _lines.FindIndex(start + 1, x => x.StartsWith("*** "));
            if (start < 0 || end < 0)
            {
                return dataSets;
            }
            var section = _lines.GetRange(start, end - start);
            for (int i = start; i < end; i++)
            {
                if (Regex.IsMatch(_lines[i], @"^\w.*"))
                {
                    dataSets.Add(new DataSet() { Name = _lines[i].Trim(), Inputs = GetInputs(_lines, i + 1) });
                }
            }
            return dataSets;
        }

        /// <summary>
        /// Returns a list of test steps for the section included in the [start, end] range.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private List<TestStep> GetSectionSteps(int start, int end)
        {
            var steps = new List<TestStep>();
            if (start < 0 || end < 0)
            {
                return steps;
            }
            start += 1;

            for (int n = start; n <= end; n++)
            {
                if (Regex.IsMatch(_lines[n], @"^\w.*"))
                {
                    steps.Add(new TestStep() { Keyword = _lines[n].Trim(), Inputs = GetInputs(_lines, n + 1) });
                }
            }
            return steps;
        }

        private Dictionary<string, string> GetInputs(List<string> lines, int index)
        {
            var inputs = new Dictionary<string, string>();
            if (lines.Count > index)
            {
                var section = _lines.GetRange(index, _lines.Count - index);
                for (int i=0; i <section.Count; i++)
                {
                    if (Regex.IsMatch(section[i], @"^\s{4}\w.*"))
                    {
                        var p = section[i].TrimStart(' ').Split(new[] { "    " }, StringSplitOptions.None);
                        if (p.Length < 2)
                        {
                            inputs.Add(p[0].Trim(), "");
                        }
                        else
                        {
                            int n = 1;
                            // check for more input on the next line using the '...' syntax
                            while ((section.Count > i+n) && Regex.IsMatch(section[i+n], "\\.{3}\\s.*"))
                            {
                                p[1] += section[i+n].Substring(4);
                                n += 1;
                                
                            }
                            inputs.Add(p[0].Trim(), p[1]);
                        }
                    }
                    else if (Regex.IsMatch(section[i], @"^\w.*"))
                    {
                        break;
                    }
                    
                }
            }
            return inputs;
        }

        /// <summary>
        /// Returns a list of test steps for the section included in the [start, end] range.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private List<TestStep> GetKeywordSubsteps(int start, int end)
        {
            var steps = new List<TestStep>();
            if (start < 0 || end < 0)
            {
                return steps;
            }
            start += 1;

            for (int n = start; n <= end; n++)
            {
                if (Regex.IsMatch(_lines[n], @"^\s{4}\w.*"))
                {
                    steps.Add(new TestStep() { Keyword = _lines[n].Trim(), Inputs = GetKeywordInputs(_lines, n + 1) });
                }
                else
                {
                    break;
                }
            }
            return steps;
        }

        private Dictionary<string, string> GetKeywordInputs(List<string> lines, int index)
        {
            var inputs = new Dictionary<string, string>();
            if (lines.Count > index)
            {
                var section = _lines.GetRange(index, _lines.Count - index);
                for (int i = 0; i < section.Count; i++)
                {
                    if (Regex.IsMatch(section[i], @"^\s{8}\w.*"))
                    {
                        var p = section[i].TrimStart(' ').Split(new[] { "    " }, StringSplitOptions.None);
                        if (p.Length < 2)
                        {
                            inputs.Add(p[0].Trim(), "");
                        }
                        else
                        {
                            int n = 1;
                            // check for more input on the next line using the '...' syntax
                            while ((section.Count > i + n) && Regex.IsMatch(section[i + n], "\\.{3}\\s.*"))
                            {
                                p[1] += section[i + n].Substring(4);
                                n += 1;

                            }
                            inputs.Add(p[0].Trim(), p[1]);
                        }
                    }
                    else //if (Regex.IsMatch(section[i], @"^\s{4}\w.*"))
                    {
                        break;
                    }

                }
            }
            return inputs;
        }
    }
}
