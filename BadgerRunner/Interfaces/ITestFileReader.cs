using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner.Interfaces
{
    public interface ITestFileReader
    {
        void LoadFile(string path);
        string GetLibraryName();
        Dictionary<string, string> GetVariables();
        List<TestStep> GetTestSteps();
        List<TestStep> GetSetup();
        List<TestStep> GetTeardown();
        List<CustomKeyword> GetKeywords();
        List<DataSet> GetDataSets();
    }
}
