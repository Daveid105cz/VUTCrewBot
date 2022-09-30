using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Repository;

namespace VUTCrewBot.Commands
{
    public class InitCommands
    {
        public ILogger<Worker> Logger { private get; set; }
        public CrewBot Bot { private get; set; }
        public BotSettings Settings { get; set; }
        public InitCommands(ILogger<Worker> logger, CrewBot bot, BotSettings settings) 
        {
            Settings = settings;
            Bot = bot;
            Logger = logger;
        }
        public void Init()
        {
            Bot.Client.MessageCreated += Client_MessageCreated;
            
        }

        private async Task Client_MessageCreated(DSharpPlus.DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e)
        {
            if (e.Message.MessageType == DSharpPlus.MessageType.Default)
            {
                if (e.Message.Content == "?initbot")
                {
                    //e.Message.RespondAsync("Id: " + e.Message.Author.Id);

                    if (e.Author.Id == 401020216655740929u || e.Author.Id == 630039282886901811u)
                    {
                        _ = Task.Factory.StartNew(() =>
                        {
                            InitTheBot(e.Message);
                        });
                    }
                    else
                    {
                        await e.Message.RespondAsync("Not dávid");
                    }

                }
                
            }
        }
        private async void InitTheBot(DiscordMessage msg)
        {
            await msg.RespondAsync("Inting");
            Settings.BotAdminNotifyMessagesChannel = new Tuple<ulong, ulong>(msg.Channel.Guild.Id ,msg.Channel.Id);
            
           // await Bot.RegisterSlashCommands();
        }
        //[Command("notifyhere")]
        //[RequireOwner]
        //public async Task MessagesHereSetCommand(CommandContext ctx)
        //{
        //    SettingsProvider.BotAdminNotifyMessagesChannel = new Tuple<ulong, ulong>(ctx.Guild.Id, ctx.Channel.Id);
        //    await SettingsProvider.SaveAsync();
        //    await ctx.RespondAsync("Všechny zprávy informující o stavu bota budou posílány do tohoto kanálu");
        //}
        //[Command("status")]
        //[RequireOwner]
        //public async Task StatusCommand(CommandContext ctx)
        //{
        //    await ctx.RespondAsync("Ima up and runnin");
        //}



    }
}
