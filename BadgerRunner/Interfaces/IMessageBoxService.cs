using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Badger.Runner.Interfaces
{
    public interface IMessageBoxService
    {
        DialogResult Show(string message, string caption, MessageBoxButtons buttons);
    }
}
