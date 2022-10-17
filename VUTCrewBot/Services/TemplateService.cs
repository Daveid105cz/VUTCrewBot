using DSharpPlus.Entities;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL.Entities;
using VUTCrewBot.Exceptions;
using VUTCrewBot.Models;
using VUTCrewBot.Commands;

namespace VUTCrewBot.Services
{
    public class TemplateService : BotService
    {
        public TemplateService(ILogger<TemplateService> logger,  ServiceContext ctx) : base(logger, ctx)
        {

        }

        public async override Task Init()
        {
            //CronJob();
        }
        public async Task<Tuple<DateTime,List<MeetTemplateModelDetail>>> GetTemplatesForTomorrow()
        {
            DateTime tomorrowDay = DateTime.Now.AddDays(1);
            DayOfWeek dayOfTomorrow = tomorrowDay.DayOfWeek;
            await using var Repo = RepositoryFactory.Create();

            var templates = await Repo.Templates.GetGeneratedTemplatesOnDay(dayOfTomorrow);
            return new Tuple<DateTime, List<MeetTemplateModelDetail>>(tomorrowDay, templates);
        }

        public async Task CreateTemplateAsync(String name, TimeSpan timeOfDay)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetTemplateModel model = await Repo.Templates.CreateTemplate(new MeetTemplateModel()
            {
                Name = name,
                MeetupDayTime = timeOfDay
            });
            await Repo.CommitAsync();
        }
        public async Task<String> ModifyTemplateAsync(int id, String? newName, TimeSpan? timeOfDay)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetTemplateModelDetail model = await Repo.Templates.GetTemplateById(id);
            if (model == null)
            {
                throw new InvalidTemplateIdException();
            }

            model.Name = newName ?? model.Name;
            model.MeetupDayTime = timeOfDay ?? model.MeetupDayTime;

            await Repo.Templates.UpdateTemplate(model);
            await Repo.CommitAsync();
            return model.Name;
        }
        public async Task DeleteTemplateAsync(int id)
        {
            await using var Repo = RepositoryFactory.Create();
            if (!await Repo.Templates.DeleteTemplate(id))
                throw new InvalidTemplateIdException();

            await Repo.CommitAsync();
        }
        public async Task<MeetTemplateModelDetail> GetTemplate(int id)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetTemplateModelDetail model = await Repo.Templates.GetTemplateById(id);
            return model;
        }
        public async Task<String> AddUserToTemplate(int id, ulong userId)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetTemplateModelDetail model = await Repo.Templates.GetTemplateById(id);
            if (model == null)
            {
                throw new InvalidTemplateIdException();
            }

            if (model.HasUser(userId))
            {
                throw new ServiceException("Tento uživatel je v templatu již přidán");
            }


            model.Users.Add(new MeetTemplateUserEntity
            {
                UserId = userId
            });
            await Repo.Templates.UpdateTemplate(model);
            await Repo.CommitAsync();
            return model.Name;
        }
        public async Task<String> RemoveUserFromTemplate(int id, ulong userId)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetTemplateModelDetail model = await Repo.Templates.GetTemplateById(id);
            if (model == null)
            {
                throw new InvalidTemplateIdException();
            }

            if (!model.HasUser(userId))
            {
                throw new ServiceException("Tento uživatel se v templatu nenachází");
            }


            model.Users.Remove(model.GetUser(userId));
            await Repo.CommitAsync();
            return model.Name;
        }
        public async Task<String> SetAutogenerate(int id, AutoGenDayEnum autogenerate)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetTemplateModelDetail model = await Repo.Templates.GetTemplateById(id);
            if (model == null)
            {
                throw new InvalidTemplateIdException();
            }

            model.DoGenerate = (autogenerate != AutoGenDayEnum.nikdy);
            model.RepeatDay = autogenerate.ToDayOfWeek();

            await Repo.Templates.UpdateTemplate(model);

            await Repo.CommitAsync();
            return model.Name;
        }
    }
}
