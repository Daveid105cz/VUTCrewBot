using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Repository;
using VUTCrewBot.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Xml.Linq;
using VUTCrewBot.Misc;
using VUTCrewBot.Services;
using VUTCrewBot.Exceptions;
using System.ComponentModel;
using Microsoft.Extensions.Logging;
using BotToolkit.BaseTypes;
using VUTCrewBot.Services.Reminders;

namespace VUTCrewBot.Commands
{
    [SlashCommandGroup("remind", "Organizace přípomínek")]
    public class ReminderCommands : MyBaseCommandModule
    {
        RemindService ReminderService { get; set; }
 
        public ReminderCommands(RemindService service,  
            ILogger<ReminderCommands> logger) : base(logger)
        {
            ReminderService = service;
        }

        [SlashCommand("create", "Vytvoří novou přípomínku s textem pro uživatele v určitý čas")]
        public async Task CreateReminderCommand(InteractionContext ctx,
        [Option("text", "Text připomínky")] String text,
        [Option("time", "Celý čas včetně data ve formátu DD.MM[.YYYY] HH:mm (pls dodrž formát, když ne tak smolík)")] String time,
        [Option("user","Uživatel")] DiscordUser? userToTag = null)
        {
            await ctx.Think();
            if (!time.ToDateTimeOrTodaySpan(out var tim))
            {
                await ctx.Respond("Špatný formát data+času");
                return;
            }
            if (tim <= DateTime.Now)
            {
                await ctx.Respond("Připomínku **NELZE** mít v minulosti");
                return;
            }
            if(userToTag== null)
            {
                userToTag = ctx.User;
            }
            await ReminderService.CreateReminderAsync(text, tim, userToTag.Id, ctx.Channel.Id);

            await ctx.Respond($"Připomínka s textem {text} přidána. Čas připomenutí: {tim:dd.MM.yyyy HH:mm}");
        }


        [SlashCommand("modify", "Změní existující přípomínku")]
        public async Task ModifyMeetCommand(InteractionContext ctx,
        [Autocomplete(typeof(ActiveReminderChoiceProvider))]
        [Option("reminder", "Měněná připomínka")] long id,
        [Option("text", "Nový text připomínky (pokud nezadán tak zůstane stejný)")] String? text = null,
        [Option("time", "Celý čas včetně data ve formátu DD.MM.YYYY HH:mm (pls dodrž formát, když ne tak smolík)")] String time = "")
        {
            await ctx.Think();
            DateTime? toChange = null;
            if(!String.IsNullOrEmpty(time))
            {
                if (!time.ToDateTimeOrTodaySpan(out var tim))
                {
                    await ctx.Respond("Špatný formát data+času");
                    return;
                }
                if (tim <= DateTime.Now)
                {
                    await ctx.Respond("Připomínku **NELZE** mít v minulosti");
                    return;
                }
                toChange = tim;
            }
            
            try
            {
                var newName = await ReminderService.ModifyReminderAsync((int)id, text,toChange);
                await ctx.Respond($"Sraz {newName} modifikován.");
            }
            catch(ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }


        [SlashCommand("delete", "Odstraní existující připomínku")]
        public async Task DeleteReminderCommand(InteractionContext ctx,
        [Autocomplete(typeof(ActiveReminderChoiceProvider))]
        [Option("reminder", "Odstraněná připomínka")] long id)
        {
            await ctx.Think();
            try
            {
                await ReminderService.DeleteReminderAsync((int)id);
                await ctx.Respond($"Připomínka odstraněna.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

            
        }



      

    }




    public class ActiveReminderChoiceProvider : IAutocompleteProvider
    {
        public static IServiceProvider services;

        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            IRepositoryFactory factory = services.GetService<IRepositoryFactory>();
            await using var repo = factory.Create();

            var reminders = await repo.Reminders.GetAllUpcomingReminders();


            return new List<DiscordAutoCompleteChoice>(
                reminders.Select(x => new DiscordAutoCompleteChoice($"{x.Text} - { x.RemindTime:dd.MM.yyyy HH:mm}", (long)x.Id))
                );
            
        }
    }


}
