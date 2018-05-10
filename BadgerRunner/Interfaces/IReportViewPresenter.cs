using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Badger.Runner.Interfaces
{
    public interface IReportViewPresenter : IViewPresenter
    {
        string Url { get; set; }
    }
}
