using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using BotToolkit.BaseTypes;

namespace VUTCrewBot
{
    public class BotConfiguration:BotConfigurationBase
    {
        public String DbConnectionString { get; set; } = string.Empty;
        //public String Token { get; set; } = string.Empty;
        //public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
    public static class BotConfigurationLoader
    {
        public static BotConfiguration LoadConfiguration()
        {
            string configFile = "BotConfig.json";
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile);
            if (!File.Exists(path))
            {
                genConfig(path);
            }
            String value = File.ReadAllText(path);
            if(value.Length==0)
            {
                genConfig(path);
            }
            var config = new ConfigurationBuilder()
                            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                            .AddJsonFile("BotConfig.json").Build();

            var section = config.GetSection(nameof(BotConfiguration));

            return section.Get<BotConfiguration>();
        }
        private static void genConfig(String path)
        {
            BotConfiguration freshConfig = new BotConfiguration();
            JObject obj = new JObject();
            obj["BotConfiguration"] = JObject.FromObject(freshConfig);
            string serialized = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(path, serialized);
        }
    }
    
}
