//using DSharpPlus.CommandsNext;
//using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot;
using VUTCrewBot.Misc;
using VUTCrewBot.Repository;

namespace ZbytkyBot.Commands
{

    //[MyModuleLifespan]
    public abstract class MyBaseCommandModule : ApplicationCommandModule
    {
        public ILogger Logger { get; set; }
        public MyBaseCommandModule(ILogger logger)
        {
            Logger = logger;
        }

    }
}
