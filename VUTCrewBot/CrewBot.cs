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
    public class CrewBot:DiscordBot
    {
        private BotSettings Settings { get; set; }

        InitCommands initCommands;

        public CrewBot(DiscordClient discordClient, ILoggerFactory loggerFactory, 
            IServiceProvider provider, BotSettings settings):base(discordClient, loggerFactory,provider)
        {
            Settings = settings;
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
            await Disconnect();
        }
        public async Task RunBot()
        {
            await Settings.LoadAsync();

            initCommands.Init();

            //Services
            RegisterService<MeetService>();
            RegisterService<TemplateService>();
            RegisterService<MeetNotifier>();

            //Commands
            RegisterCommand<MeetCommands>();
            RegisterCommand<MeetTemplateCommands>();

            //Scheduled jobs
            RegisterJob<MeetRemindJob>("MeetReminding", "0 * * * *");
            RegisterJob<TemplateGenerationJob>("TemplateGenerator", "0 23 * * *");

            await Connect();
        }
        public override Task OnBotWentUp()
        {
            PrintBotUpMessage();
            return base.OnBotWentUp();
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
