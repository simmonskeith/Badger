using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner.Interfaces
{
    public interface ITestService
    {

        bool Init(string path, string resourcePath);

        bool RunTestSteps();

        bool RunSetup();

        bool RunTearDown();
    }
}
