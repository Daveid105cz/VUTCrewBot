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
using VUTCrewBot.Misc;
using DSharpPlus.SlashCommands;
using VUTCrewBot.Commands;
using VUTCrewBot.Services;

namespace VUTCrewBot
{
    public class CrewBot
    {
        public DiscordClient Client { get; set; }
        private BotConfiguration Configuration { get; set; }
        private ILogger<Worker> Logger { get; set; }
        private BotSettings Settings { get; set; }


        private readonly List<BotService> _botServices = new();

        IServiceProvider _provider;

        InitCommands initCommands;

        public CrewBot(BotConfiguration configuration, DiscordBotLibLoggerFactory logFactory, IServiceProvider provider, BotSettings settings)
        {
            _provider = provider;
            Configuration = configuration;
            Logger = logFactory.CreateLogger<Worker>();
            Settings = settings;
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = configuration.Token,
                TokenType = TokenType.Bot,
                LoggerFactory = logFactory,
                MinimumLogLevel = LogLevel.Trace,
                
            });
            initCommands = new InitCommands(Logger, this, Settings);
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


            await RegisterSlashCommands();
            Client.Ready += Client_Ready;
            Client.ClientErrored += Client_ClientErrored;
            
            await Client.ConnectAsync();
            
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

        private async Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e)
        {
            Client.Logger.LogInformation("BOT ready");

            foreach (BotService bs in _botServices)
            {
                await bs.Init(sender);
            }

            _ = Task.Run(async () =>
            {
                await PrintBotUpMessage();
                //SettingsProvider.LastVersionCommitNumber = informationalVersion;
                //await SettingsProvider.SaveAsync();
            });
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

            var channel = await Client.GetChannelAsync(channelToPrintTo.Item2);
            //bool isNew = informationalVersion != SettingsProvider.LastVersionCommitNumber;
            await channel.SendMessageAsync("Bot úspěšně spušťen.");
        }
        private async Task PrintBotDownMessage()
        {
            var channelToPrintTo = Settings.BotAdminNotifyMessagesChannel;
            if (channelToPrintTo == null)
                return;
            var channel = await Client.GetChannelAsync(channelToPrintTo.Item2);
            await channel.SendMessageAsync("Bot se vypíná");
        }
    }
}
