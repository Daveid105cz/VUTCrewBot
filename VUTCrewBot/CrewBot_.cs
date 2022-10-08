using DSharpPlus.Entities;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Repository;
using DSharpPlus.SlashCommands;
using VUTCrewBot.Commands;
using VUTCrewBot.Services;
using VUTCrewBot.BotToolkit;

namespace VUTCrewBot
{
    public class CrewBotO
    {
        public DiscordClient Client { get; set; }
        private BotConfiguration Configuration { get; set; }
        private ILogger<Worker> Logger { get; set; }
        private BotSettings Settings { get; set; }
        private JobScheduler Jobs { get; set; }

        private readonly List<BotService> _botServices = new();

        IServiceProvider _provider;

        InitCommands initCommands;

        public CrewBotO(BotConfiguration configuration, DiscordBotLibLoggerFactory logFactory, 
            IServiceProvider provider, BotSettings settings, 
            JobScheduler jobScheduler)
        {
            ILoggerFactory af = provider.GetService<ILoggerFactory>();
            _provider = provider;
            Configuration = configuration;
            Logger = logFactory.CreateLogger<Worker>();
            Settings = settings;
            Jobs = jobScheduler;
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = configuration.Token,
                TokenType = TokenType.Bot,
                LoggerFactory = logFactory,
                MinimumLogLevel = LogLevel.Trace,
                
            });
            //initCommands = new InitCommands(Logger, this, Settings);
            MeetTemplatesChoiceProvider.services = provider;
            ActiveMeetChoiceProvider.services = provider;
        }
        public void DisposeBot()
        {
            Client?.Dispose();
        }
        public async Task Stop()
        {
            //foreach (BotService bs in _botServices)
            //{
            //    await bs.Stop();
            //}
            await PrintBotDownMessage();
            await Client.DisconnectAsync();
        }
        public async Task RunBot()
        {
            //await SettingsProvider.LoadAsync();
            await Settings.LoadAsync();

            initCommands.Init();

            RegisterService<MeetService>();
            RegisterService<TemplateService>();
            Jobs.RegisterJob<MeetRemindJob>("MeetReminding","0 * * * *");

            await RegisterSlashCommands();
            Client.Ready += Client_Ready;
            Client.ClientErrored += Client_ClientErrored;

            await Client.ConnectAsync();
            
        }

        private async Task Client_Resumed(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Client.Logger.LogInformation("Resumedddddddddd");
        }

        private async Task Client_ClientErrored(DiscordClient sender, DSharpPlus.EventArgs.ClientErrorEventArgs e)
        {
            Client.Logger.LogError(e.Exception.Message);
            Client.Logger.LogError(e.Exception.StackTrace);
        }

        public async Task RegisterSlashCommands()
        {
            var slashCommands = Client.UseSlashCommands(new SlashCommandsConfiguration()
            {
                Services = _provider,
                
            });
            slashCommands.SlashCommandErrored += SlashCommands_SlashCommandErrored;
            ulong? guildId = null;
#if DEBUG
            guildId = 700426862245183580u;
#endif
            slashCommands.RegisterCommands<MeetCommands>(guildId);
            slashCommands.RegisterCommands<MeetTemplateCommands>(guildId);
            
        }

        private async Task SlashCommands_SlashCommandErrored(SlashCommandsExtension sender, DSharpPlus.SlashCommands.EventArgs.SlashCommandErrorEventArgs e)
        {
            Logger.LogError(e.Exception.Message);
            Logger.LogError(e.Exception.StackTrace);
        }
        bool isFirst = false;
        private async Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Client.Logger.LogInformation("BOT ready");
            if(!isFirst)
            {
                isFirst = true;
                foreach (BotService bs in _botServices)
                {
                    await bs.Init(sender);
                }
                Jobs.Work();
                
            }

            PrintBotUpMessage();
            
        }
        private void RegisterService<T>() where T : BotService
        {
            BotService bs = _provider.GetService<T>();
            _botServices.Add(bs);
        }
        private async Task PrintBotUpMessage()
        {
            var channelToPrintTo = Settings.BotAdminNotifyMessagesChannel;
            if (channelToPrintTo == null)
                return;

            var channel = await Client.GetChannelAsync(channelToPrintTo.Value.Id);
            //bool isNew = informationalVersion != SettingsProvider.LastVersionCommitNumber;
            await channel.SendMessageAsync("Bot úspěšně spušťen.");
        }
        private async Task PrintBotDownMessage()
        {
            var channelToPrintTo = Settings.BotAdminNotifyMessagesChannel;
            if (channelToPrintTo == null)
                return;
            var channel = await Client.GetChannelAsync(channelToPrintTo.Value.Id);
            await channel.SendMessageAsync("Bot se vypíná");
        }
    }
}
