using DSharpPlus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.BotToolkit
{
    public class DiscordBotLibLoggerFactory : ILoggerFactory
    {
        private ILogger _logger;
        public DiscordBotLibLoggerFactory(ILogger<DiscordClient> logger)
        {
            _logger = logger;
        }
        public void AddProvider(ILoggerProvider provider)
        {
            throw new NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _logger;
        }

        public void Dispose()
        {
            _logger = null;
        }
    }
}
