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
        public TemplateService(IServiceProvider serviceProvider, IRepositoryFactory repoFactory) : base(serviceProvider, repoFactory)
        {

        }

        public async override Task Init()
        {
            CronJob();
        }
        private async Task CronJob()
        {
            //DateTime now = new DateTime(2022, 10, 3, 23, 0, 20);
            DateTime now = DateTime.Now;
            bool appendDay = now.Hour >= 23;
            DateTime nearestDayEnd = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0).AddHours(23);
            nearestDayEnd = nearestDayEnd.AddDays(appendDay?1:0);
            Logger.LogInformation("Scheduling template generation cronjob at " + nearestDayEnd.ToString());
            await Tasc.WaitUntil(nearestDayEnd);
            Logger.LogInformation("Running the CRON job");
            //RUN the job
            Console.WriteLine("testing templatess");
            _ = Task.Factory.StartNew(() =>
            {
                CronJob();
            });

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
