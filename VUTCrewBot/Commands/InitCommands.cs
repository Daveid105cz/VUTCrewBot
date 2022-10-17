using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;
using VUTCrewBot.Repository;

namespace VUTCrewBot.Commands
{
    public class InitCommands
    {
        public ILogger Logger { private get; set; }
        public CrewBot Bot { private get; set; }
        public BotSettings Settings { get; set; }
        IServiceProvider ServiceProvider { get; set; }
        public InitCommands(ILogger logger, CrewBot bot, BotSettings settings, IServiceProvider serviceProvider) 
        {
            Settings = settings;
            Bot = bot;
            Logger = logger;
            ServiceProvider = serviceProvider;
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
                else if(e.Message.Content == "?meethere")
                {
                    if (e.Author.Id == 401020216655740929u || e.Author.Id == 630039282886901811u)
                    {
                        Settings.MeetsNotifyChannel = new ChannelId(e.Message.Channel.Guild.Id, e.Message.Channel.Id);
                        await e.Message.CreateReactionAsync(DiscordEmoji.FromName(Bot.Client, ":thumbsup:"));
                    }
                    else
                    {
                        await e.Message.RespondAsync("Not dávid");
                    }
                }
                else if (e.Message.Content == "?dbdump")
                {
                    if (e.Author.Id == 401020216655740929u || e.Author.Id == 630039282886901811u)
                    {
                        //String dbpath = Configuration.DbConnectionString.Split("=")[1];
                        IBotDbContextFactory fact = ServiceProvider.GetService<IBotDbContextFactory>();
                        var dbcx = await fact.CreateDbContextAsync();
                        try
                        {
                            String dbPath = "lastDbBackup.db";
                            File.Delete(dbPath);

                            await dbcx.Database.ExecuteSqlRawAsync($"VACUUM INTO '{dbPath}'");

                            using (var fs = new FileStream(dbPath, FileMode.Open, FileAccess.Read))
                            {
                                var msg = await new DiscordMessageBuilder()
                                    .WithContent("DB dump here")
                                    .WithFiles(new Dictionary<string, Stream>() { { "dbdump.db", fs } })
                                    .SendAsync(e.Message.Channel);
                            }
                        }
                        catch (Exception ex)
                        {

                            throw ex;
                        }
                        finally
                        {
                            dbcx.Dispose();
                        }

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
            Settings.BotAdminNotifyMessagesChannel = new ChannelId(msg.Channel.Guild.Id, msg.Channel.Id);
            //new Tuple<ulong, ulong>(msg.Channel.Guild.Id ,msg.Channel.Id);

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
