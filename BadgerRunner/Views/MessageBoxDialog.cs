using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Badger.Runner.Interfaces;

namespace Badger.Runner.Views
{
    public class MessageBoxDialog : IMessageBoxService
    {
        public DialogResult Show(string message, string caption, MessageBoxButtons buttons)
        {
            return MessageBox.Show(message, caption, buttons);
        }
    }
}
