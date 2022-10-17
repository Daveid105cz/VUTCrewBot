using Microsoft.Extensions.Logging;

namespace BotToolkit.BaseTypes
{
    public class BotConfigurationBase
    {
        public string Token { get; set; } = string.Empty;
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
