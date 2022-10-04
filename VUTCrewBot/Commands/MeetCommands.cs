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
using ZbytkyBot.Commands;
using VUTCrewBot.Misc;
using VUTCrewBot.Services;
using VUTCrewBot.Exceptions;
using System.ComponentModel;

namespace VUTCrewBot.Commands
{
    [SlashCommandGroup("meet", "Organizace meetů")]
    //[SlashModuleLifespan(SlashModuleLifespan.Scoped)]
    public class MeetCommands : MyBaseCommandModule
    {
        MeetService MeetService { get; set; }
        public MeetCommands(IRepositoryFactory factory,MeetService service) : base(factory)
        {
            MeetService = service;
        }

        [SlashCommand("create", "Vytvoří nový sraz s názvem a časem")]
        public async Task CreateMeetCommand(InteractionContext ctx,
        [Option("name", "Název srazu")] String name,
        [Option("time", "Celý čas včetně data ve formátu DD.MM[.YYYY] HH:mm (pls dodrž formát, když ne tak smolík)")] String time)
        {
            await ctx.Think();
            if(!time.ToFullDate(out var tim))
            {
                await ctx.Respond("Špatný formát data+času");
                return;
            }
            if(tim <= DateTime.Now)
            {
                await ctx.Respond("Sraz **NELZE** mít v minulosti");
                return;
            }

            await MeetService.CreateMeetAsync(name, tim);

            await ctx.Respond($"Sraz {name} přidán. v čas: {tim:dd.MM.yyyy HH:mm}");
        }


        [SlashCommand("modify", "Změní existující sraz")]
        public async Task ModifyMeetCommand(InteractionContext ctx,
        [Autocomplete(typeof(ActiveMeetChoiceProvider))]
        [Option("meet", "Měněný sraz")] long id,
        [Option("name", "Nový název srazu (pokud nezadán tak zůstane stejný)")] String? name = null,
        [Option("time", "Celý čas včetně data ve formátu DD.MM.YYYY HH:mm (pls dodrž formát, když ne tak smolík)")] String time = "")
        {
            //TODO: move repository logic to the service
            await ctx.Think();
            DateTime? toChange = null;
            if(!String.IsNullOrEmpty(time))
            {
                if (!time.ToFullDate(out var tim))
                {
                    await ctx.Respond("Špatný formát data+času");
                    return;
                }
                if (tim <= DateTime.Now)
                {
                    await ctx.Respond("Sraz **NELZE** mít v minulosti");
                    return;
                }
                toChange = tim;
            }
            
            try
            {
                var newName = await MeetService.ModifyMeetAsync((int)id, name, toChange);
                await ctx.Respond($"Sraz {newName} modifikován.");
            }
            catch(ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }


        [SlashCommand("delete", "Odstraní existující sraz")]
        public async Task DeleteMeetCommand(InteractionContext ctx,
        [Autocomplete(typeof(ActiveMeetChoiceProvider))]
        [Option("meet", "Odstraněný sraz")] long id)
        {
            await ctx.Think();
            try
            {
                await MeetService.DeleteMeetAsync((int)id);
                await ctx.Respond($"Sraz odstraněn.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

            
        }



        [SlashCommand("useradd", "Přidá uživatele do srazu")]
        public async Task AddUserCommand(InteractionContext ctx,
        [Autocomplete(typeof(ActiveMeetChoiceProvider))]
        [Option("meet", "Modifikovaný sraz")] long id,
        [Option("user", "Přidaný uživatel")] DiscordUser user)
        {
            await ctx.Think();
            try
            {
                var meetName = await MeetService.AddUserToMeet((int)id, user.Id);
                await ctx.Respond($"Uživatel {user.Username} přidán do srazu {meetName}.");
            }
            catch(ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }


        [SlashCommand("userdel", "Odstraní uživatele ze srazu")]
        public async Task DeleteUserCommand(InteractionContext ctx,
        [Autocomplete(typeof(ActiveMeetChoiceProvider))]
        [Option("meet", "Modifikovaný sraz")] long id,
        [Option("user", "Odstraněný uživatel")] DiscordUser user)
        {
            await ctx.Think();
            try
            {
                var meetName = await MeetService.RemoveUserFromMeet((int)id, user.Id);
                await ctx.Respond($"Uživatel {user.Username} odstraněn ze srazu {meetName}.");
            }
            catch (ServiceException ex)
            {
                await ctx.Respond(ex.Message);
            }

        }


        [SlashCommand("from", "Vytvoří sraz dle existujícího templatu")]
        public async Task FromTemplateCommand(InteractionContext ctx,
        [Autocomplete(typeof(MeetTemplatesChoiceProvider))]
        [Option("template", "Template srazu")] long templateId)
        {
            await ctx.Think();


            await ctx.Respond($"Template forknut {0}.");
        }

    }




    public class ActiveMeetChoiceProvider : IAutocompleteProvider
    {
        public static IServiceProvider services;

        public async Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext ctx)
        {
            IRepositoryFactory factory = services.GetService<IRepositoryFactory>();
            await using var repo = factory.Create();

            var meets = await repo.Meet.GetAllMeets();


            return new List<DiscordAutoCompleteChoice>(
                meets.Select(x => new DiscordAutoCompleteChoice(x.Name + " - "+x.MeetupTime, (long)x.Id))
                );
            
        }
    }

    public enum AutoGenDayEnum
    {
        [ChoiceName("Pondělí")]
        [Description("Pondělí")]
        pondeli,
        [ChoiceName("Úterý")]
        [Description("Úterý")]
        utery,
        [ChoiceName("Středa")]
        [Description("Středa")]
        streda,
        [ChoiceName("Čtvrtek")]
        [Description("Čtvrtek")]
        ctvrtek,
        [ChoiceName("Pátek")]
        [Description("Pátek")]
        patek,
        [ChoiceName("Sobota")]
        [Description("Sobota")]
        sobota,
        [ChoiceName("Neděle")]
        [Description("Neděle")]
        nedela,
        [ChoiceName("Nikdy")]
        [Description("Nikdy")]
        nikdy,

        
    }
}
