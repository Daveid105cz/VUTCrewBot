using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Commands;
using VUTCrewBot.Services;

namespace VUTCrewBot.BotToolkit
{
    public class DiscordBot<TConf> where TConf:BotConfigurationBase
    {
        protected IServiceProvider ServiceProvider { get; set; }
        public DiscordClient Client { get; set; }
        protected ILogger<DiscordBot<TConf>> Logger { get; set; }
        protected JobScheduler Jobs { get; set; }
        protected BotConfigurationBase Configuration { get; set; }
        protected List<BotService> BotServices { get; set; } = new();
        protected SlashCommandsExtension SlashCommands { get; set; }

        public DiscordBot(ILoggerFactory loggerFactory, 
            IServiceProvider provider,
            TConf configuration)
        {
            ServiceProvider = provider;
            Configuration = configuration;
            Logger = loggerFactory.CreateLogger<DiscordBot<TConf>>();
            Jobs = new JobScheduler(ServiceProvider);
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = configuration.Token,
                TokenType = TokenType.Bot,
                LoggerFactory = loggerFactory,
                MinimumLogLevel = configuration.LogLevel

            });
            Client.Ready += Client_Ready;
            Client.ClientErrored += Client_ClientErrored;

            SlashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = provider
            });
            SlashCommands.SlashCommandErrored += SlashCommands_SlashCommandErrored;
        }



        public async virtual Task OnBotWentUp()
        {

        }
        public async virtual Task OnBotGoingDown()
        {

        }

        public void RegisterService<T>() where T : BotService
        {
            var bs = ServiceProvider.GetService<T>();
            if (bs == null)
                throw new Exception("Service not found in DI container");
            BotServices.Add(bs);
        }
        public void RegisterCommand<T>() where T : ApplicationCommandModule
        {
            ulong? guildId = null;
#if DEBUG
            guildId = 700426862245183580u;
#endif
            SlashCommands.RegisterCommands<T>(guildId);
        }
        public void RegisterJob<T>(String name, String cron)
        {

        }
        
        protected async Task Connect()
        {
            await Client.ConnectAsync();
        }
        protected async Task Disconnect()
        {
            await OnBotGoingDown();
            await Client.DisconnectAsync();
        }
        private async Task Client_ClientErrored(DiscordClient sender, DSharpPlus.EventArgs.ClientErrorEventArgs e)
        {
            
        }
        bool firstReady = true;
        private async Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            if(firstReady)
            {
                firstReady = false;
                foreach (BotService bs in BotServices)
                {
                    await bs.Init(sender);
                }
                Jobs.Work();

                OnBotWentUp();
            }
        }
        private async Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
        {
            Logger.LogError(e.Exception.Message);
            Logger.LogError(e.Exception.StackTrace);
        }
    }
}
