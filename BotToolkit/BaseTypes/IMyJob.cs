using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotToolkit.BaseTypes
{
    public interface IMyJob
    {
        public Task RunAsync();
    }
}
