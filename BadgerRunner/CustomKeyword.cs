using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner
{
    public class CustomKeyword
    {
        public string Keyword { get; set; }
        public List<TestStep> Steps { get; set; }
        public List<string> Inputs { get; set; }
    }
}
