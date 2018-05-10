using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner
{
    public class TestStep
    {
        public string Keyword { get; set; }
        public Dictionary<string, string> Inputs { get; set; }
        public bool IsSetup { get; set; }
        public bool IsTeardown { get; set; }
    }
}
