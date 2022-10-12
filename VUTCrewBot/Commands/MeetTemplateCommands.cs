using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Exceptions;
using VUTCrewBot.Services;
using ZbytkyBot.Commands;

namespace VUTCrewBot.Commands
{
    [SlashCommandGroup("template", "Organizace templatů")]
    public class MeetTemplateCommands : MyBaseCommandModule
    {
        TemplateService TemplateService { get; set; }
        public MeetTemplateCommands(TemplateService service, ILogger<MeetTemplateCommands> logger) : base(logger)
        {
            TemplateService = service;
        }

        [SlashCommand("create", "Vytvoří nový template s názvem a časem")]
        public async Task CreateTemplateCommand(InteractionContext ctx,
         [Option("name", "Název srazu/templatu")] String name,
         [Option("time", "Čas ve formátu HH:MM (pls dodrž formát, když ne tak smolík)")] String time)
        {
            await ctx.Think();
            if (!time.ToTimeOfDay(out var timeOfDay))
            {
                await ctx.Respond($"Nevalidní formát času.");
                return;
            }

            await TemplateService.CreateTemplateAsync(name, timeOfDay);

            await ctx.Respond($"Template vytvořen v čas: {timeOfDay:hh':'mm}");
        }

        //         [Option("dayofweek", "Den v týdnu option")] AutoGenDayEnum dayOfWeek

        [SlashCommand("modify", "Změní existující template")]
        public async Task ModifyTemplateCommand(InteractionContext ctx,
        [Autocomplete(typeof(MeetTemplatesChoiceProvider))]
        [Option("template", "Měněný template")] long id,
        [Option("name", "Nový název srazu/templatu")] String? name = null,
        [Option("time", "Čas ve formátu HH:MM (pls dodrž formát, když ne tak smolík)")] String time = "")
        {
            await ctx.Think();
            TimeSpan? toChange = null;
            if (!String.IsNullOrEmpty(time))
            {
                if (!time.ToTimeOfDay(out var timeOfDay))
                {
                    await ctx.Respond($"Nevalidní formát času.");
                    return;
                }
                toChange = timeOfDay;
            }

            try
            {
                var newName = await TemplateService.ModifyTemplateAsync((int)id, name, toChange);
                await ctx.Respond($"Template {newName} modifikován.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }


        [SlashCommand("delete", "Odstraní existující template")]
        public async Task DeleteTemplateCommand(InteractionContext ctx,
        [Autocomplete(typeof(MeetTemplatesChoiceProvider))]
        [Option("template", "Odstraněný template")] long id)
        {
            await ctx.Think();
            try
            {
                await TemplateService.DeleteTemplateAsync((int)id);
                await ctx.Respond($"Template odstraněn.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }



        [SlashCommand("useradd", "Přidá uživatele do templatu")]
        public async Task AddUserCommand(InteractionContext ctx,
        [Autocomplete(typeof(MeetTemplatesChoiceProvider))]
        [Option("template", "Modifikovaný template")] long id,
        [Option("user", "Přidaný uživatel")] DiscordUser user)
        {
            await ctx.Think();
            try
            {
                var meetName = await TemplateService.AddUserToTemplate((int)id, user.Id);
                await ctx.Respond($"Uživatel {user.Username} přidán do templatu {meetName}.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }


        [SlashCommand("userdel", "Odstraní uživatele z templatu")]
        public async Task DeleteUserCommand(InteractionContext ctx,
        [Autocomplete(typeof(MeetTemplatesChoiceProvider))]
        [Option("template", "Modifikovaný teplate")] long id,
        [Option("user", "Odstraněný uživatel")] DiscordUser user)
        {
            await ctx.Think();
            try
            {
                var meetName = await TemplateService.RemoveUserFromTemplate((int)id, user.Id);
                await ctx.Respond($"Uživatel {user.Username} odstraněn z templatu {meetName}.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }

        [SlashCommand("autogen", "Nastaví automatickou generaci srazů z templatu")]
        public async Task SetTemplateAutogenerateCommand(InteractionContext ctx,
        [Autocomplete(typeof(MeetTemplatesChoiceProvider))]
        [Option("template", "Modifikovaný teplate")] long id,
        [Option("dayofweek", "Den v týdnu option")] AutoGenDayEnum dayOfWeek)
        {
            await ctx.Think();
            try
            {
                var meetName = await TemplateService.SetAutogenerate((int)id, dayOfWeek);
                await ctx.Respond($"Template {meetName} bude generován v {dayOfWeek}.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }

    }

    public class MeetTemplatesChoiceProvider : IAutocompleteProvider
    {
        public static IServiceProvider services;

        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            IRepositoryFactory factory = services.GetService<IRepositoryFactory>();
            await using var repo = factory.Create();

            var meets = await repo.Templates.GetAllTemplates();


            return new List<DiscordAutoCompleteChoice>(
                meets.Select(x => 
                    new DiscordAutoCompleteChoice(
                        x.Name + " - " + x.MeetupDayTime.ToString("hh':'mm") + " - " + x.GenerateString, 
                        (long)x.Id))
                );
        }
    }
}
