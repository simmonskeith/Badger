using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Core
{

    [AttributeUsage(AttributeTargets.Class)]
    public class StepsAttribute : System.Attribute
    {
        public readonly string Text;

        public StepsAttribute(string text)
        {
            this.Text = text;
        }
    }


    [AttributeUsage(AttributeTargets.Method)]
    public class StepAttribute : System.Attribute
    {
        public readonly string Text;

        public StepAttribute(string text)
        {
            this.Text = text;
        }
    }
}
