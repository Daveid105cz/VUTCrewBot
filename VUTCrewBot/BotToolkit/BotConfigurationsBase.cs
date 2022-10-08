using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.BotToolkit
{
    public class BotConfigurationBase
    {
        public String Token { get; set; } = string.Empty;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
