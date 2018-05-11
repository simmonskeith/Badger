using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Badger.Core;
using Badger.Core.Interfaces;
using Badger.Runner.Interfaces;
using System.Reflection;

namespace Badger.Runner
{
    public class TestService : ITestService
    {
        DateTime timeStamp;

        private ITestFileReader _testFileReader;
        private static Assembly _assembly;
        private static MethodInfo[] _steps;
        private List<TestStep> _setups;
        private List<TestStep> _testSteps;
        private List<TestStep> _teardowns;
        private IFileService _fileService;
        private List<CustomKeyword> _customSteps;
        private List<DataSet> _dataSets;

        public TestService(IFileService fileService)
        {
            _fileService = fileService;
        }

        public bool Init(string path, string resourcePath)
        {
            timeStamp = DateTime.Now;

            DataStore.Initialize();

            _testFileReader = new TestFileReader(_fileService);

            Dictionary<string, string> testVariables = new Dictionary<string, string>();
            
            // read the resource file, if given
            if (String.IsNullOrEmpty(resourcePath) == false)
            {
                _testFileReader.LoadFile(resourcePath);
                testVariables = _testFileReader.GetVariables();
            }

            
            _testFileReader.LoadFile(path);
            _assembly = Assembly.Load(_testFileReader.GetLibraryName());
            testVariables.Concat(_testFileReader.GetVariables());
            _setups = _testFileReader.GetSetup();
            _testSteps = _testFileReader.GetTestSteps();
            _teardowns = _testFileReader.GetTeardown();
            _customSteps = _testFileReader.GetKeywords();
            _dataSets = _testFileReader.GetDataSets();

            // insert the variables into the datastore to be accessed by steps
            foreach(var pair in testVariables)
            {
                DataStore.Add(pair.Key, pair.Value);
            }


            // confirm all the required steps are defined
            var allSteps = GetAvailableTestSteps();
            allSteps.AddRange(_customSteps.Select(k => k.Keyword));

            var missingSteps = _setups.Where(s => allSteps.Contains(s.Keyword) == false).ToList();
            missingSteps.AddRange(_teardowns.Where(s => allSteps.Contains(s.Keyword) == false).ToList());
            missingSteps.AddRange(_testSteps.Where(s => allSteps.Contains(s.Keyword) == false).ToList());

            if (missingSteps.Count > 0)
            {
                Console.WriteLine("Unable to exeucte the test case.");
                Console.WriteLine("The following steps do not have definintions:\n" + string.Join("\n", missingSteps.Select(s=>s.Keyword)));
                
                return false; 
            }
            

            return true;
        }

        public List<string> GetAvailableTestSteps()
        {
            return GetStepMethods().Select(m => m.GetCustomAttribute<StepAttribute>().Text).ToList();
        }

        public static MethodInfo[] GetStepMethods()
        {
            if (_steps == null)
            {
                var types = _assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(StepsAttribute)));
                _steps = _assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(StepsAttribute)))
                    .SelectMany(c => c.GetMethods()
                    .Where(m => Attribute.IsDefined(m, typeof(StepAttribute)))).ToArray();
            }
            return _steps;
        }

        public static MethodInfo GetStepMethod(string step)
        {
            return GetStepMethods().FirstOrDefault(m => m.GetCustomAttribute<StepAttribute>().Text == step);
        }

        public void CallTestStepMethod(TestStep step)
        {
            try
            {
                var method = GetStepMethod(step.Keyword);
                var expectedArgs = method.GetParameters();
                var suppliedArgs = new List<object>(expectedArgs.Length);

                // convert each paramter to the type from the method parameter list and insert into list.
                foreach (var arg in expectedArgs)
                {
                    // if the method takes a dictionary as an input, pass all the suppied parameters in as 
                    // a dictionary.
                    if (arg.ParameterType == typeof(Dictionary<string, string>))
                    {
                        var d = new Dictionary<string, string>();
                        foreach (var input in step.Inputs)
                        {
                            d[input.Key] = input.Value;
                        }
                        suppliedArgs.Add(d);
                    }
                    else
                    {
                        // locate all parameters whose name matches an argument in the method paramter list.
                        var matchingParams = step.Inputs.Where(n => n.Key == arg.Name);

                        if (matchingParams.Count() > 0)
                        {
                            suppliedArgs.Insert(arg.Position, Convert.ChangeType(matchingParams.First().Value, arg.ParameterType));
                        }
                        else
                        {
                            if (arg.HasDefaultValue)
                            {
                                suppliedArgs.Insert(arg.Position, Type.Missing);
                            }
                            else
                            {
                                suppliedArgs.Insert(arg.Position, null);
                            }
                        }
                    }
                }

                var actionInstance = Activator.CreateInstance(method.DeclaringType, new Object[] { });
                method.Invoke(actionInstance, suppliedArgs.ToArray());
            }
            catch (Exception e)
            {
                Log.Fail($"A test step exception occurred: {e.Message}, {e.StackTrace}");
            }
        }

        private void SubstituteVariables(TestStep step)
        {
            foreach (var input in step.Inputs.ToList())
            {
                var value = input.Value;
                var matches = new Regex(@"\$\{(.+?)\}").Matches(value);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        string variableName = match.Groups[1].Value;

                        if (variableName.Contains("DATETIME."))
                        {
                            variableName = variableName.Replace("DATETIME.", "arg.");
                            var tmpMethod = EvalProvider.CreateEvalMethod<DateTime, string>(@"return " + variableName + ";");
                            step.Inputs[input.Key] = value.Replace(match.Captures[0].Value, tmpMethod(timeStamp));
                        }
                        else if (DataStore.Contains(variableName))
                        {
                            step.Inputs[input.Key] = value.Replace(match.Captures[0].Value, DataStore.Get(variableName));
                        }
                        else
                        {
                            Console.WriteLine($"Unable to convert parameter {input.Key} using value {input.Value}");
                        }
                    }
                }
            }
        }

        public bool RunSetup()
        {
            return RunSteps(_setups);
        }

        public bool RunTestSteps()
        {
            return RunSteps(_testSteps);
        }

        public bool RunTearDown()
        {
            return RunSteps(_teardowns);
        }

        private bool RunSteps(List<TestStep> steps)
        {
            bool result = true;

            foreach (var step in steps)
            {
                try
                {
                    string stepName = step.Keyword;
                    if (step.IsSetup)
                    {
                        stepName = "[Setup] " + stepName;
                    }
                    else if (step.IsTeardown)
                    {
                        stepName = "[Teardown] " + stepName;
                    }

                    
                    // sub in paramters to the step
                    MergeDataSetIntoTestStep(step);
                    SubstituteVariables(step);

                    Log.StartTestStep(stepName, step.Inputs);

                    // if the step is a custom step defined in the test case or resource files, run all
                    // the nested steps.
                    var customStep = _customSteps.FirstOrDefault(c => c.Keyword == step.Keyword);
                    if (customStep != null)
                    {
                        for (var j=0; j<customStep.Steps.Count; j++)
                        {
                            MergeDataSetIntoTestStep(customStep.Steps[j]);
                            SubstituteKeywordArguments(step, customStep.Steps[j]);
                            SubstituteVariables(customStep.Steps[j]);
                            CallTestStepMethod(customStep.Steps[j]);
                        }
                    }
                    else
                    {
                        CallTestStepMethod(step);
                    }

                    Log.EndTestStep(stepName);
                    result &= Log.FailCount == 0;
                }
                catch (Exception e)
                {
                    // if the test step throws an error, report the step name, set overall result to failed
                    Console.WriteLine($"A test step execution exception occurred.");
                    Console.WriteLine("Test Step: " + step.Keyword);
                    Console.WriteLine("Error: " + e.Message);
                    Console.WriteLine("Stack Trace: " + e.StackTrace);
                    result = false;
                }
            }
            return result;
        }

        private void RunCustomStep(TestStep step)
        {
            SubstituteVariables(step);
            CallTestStepMethod(step);
        }

        /// <summary>
        /// Substitutes the custom step input values for all inputs in the child step which specify the argument.
        /// </summary>
        /// <param name="parentStep">the parent customized step, with optional inputs and associated values</param>
        /// <param name="nestedStep">a child step which may call upon one or more of the parent step arguments</param>
        private void SubstituteKeywordArguments(TestStep parentStep, TestStep nestedStep)
        {
            // for each input in the nested step, if the value associated with the input contains the name
            // of an argument to the parent step enclosed within the ${} syntax,then substitue the parent step
            // value for that argument.
            var parentInputs = parentStep.Inputs.ToList();

            foreach (var input in nestedStep.Inputs.ToList())
            {
                parentInputs.ForEach(i =>
                {
                    if (nestedStep.Inputs[input.Key].Equals($"${{{i.Key}}}"))
                    {
                        nestedStep.Inputs[input.Key] = parentStep.Inputs[i.Key];
                        return;
                    }
                });
            }
        }

        private void MergeDataSetIntoTestStep(TestStep step)
        {
            var names = step.Inputs.Where(s => s.Key.StartsWith("DataSet")).ToList();
            foreach (var name in names)
            {
                step.Inputs.Remove(name.Key);
                foreach(var item in _dataSets.FirstOrDefault(d=>d.Name == name.Value).Inputs)
                {
                    // add the data set value if the step doesn't contain the key. (step values override data set values)
                    if (!step.Inputs.Keys.Contains(item.Key))
                    {
                        step.Inputs.Add(item.Key, item.Value);
                    }
                }
            }
                
        }
    }
}
