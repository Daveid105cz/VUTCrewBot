using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL.Entities;
using VUTCrewBot.Exceptions;
using VUTCrewBot.Models;

namespace VUTCrewBot.Services.Reminders
{
    public class RemindService : BotService
    {
        public RemindService(ILogger logger, ServiceContext sc) : base(logger, sc)
        {
        }

        public delegate void ReminderUpdatedEventHandler(ReminderModel model);
        public delegate void ReminderDeletedEventHandler(int id);
        public delegate void ReminderCreatedEventHandler(ReminderModel model);

        public event ReminderUpdatedEventHandler? ReminderUpdated;
        public event ReminderDeletedEventHandler? ReminderDeleted;
        public event ReminderCreatedEventHandler? ReminderCreated;


        public async Task CreateReminderAsync(String text, DateTime time, ulong user, ulong channel)
        {
            await using var Repo = RepositoryFactory.Create();
            var newModel = new ReminderModel()
            {
                Text = text,
                RemindTime = time,
                User = user,
                Channel = channel
            };


            await Repo.Reminders.CreateReminder(newModel);
            await Repo.CommitAsync();
            ReminderCreated?.Invoke(newModel);
        }
        public async Task<String> ModifyReminderAsync(int id, String? newText, DateTime? newTime)
        {
            await using var Repo = RepositoryFactory.Create();
            ReminderModel model = await Repo.Reminders.GetReminderById((int)id);
            if (model == null)
            {
                throw new InvalidMeetIdException();
            }

            model.Text = newText ?? model.Text;
            model.RemindTime = newTime ?? model.RemindTime;

            await Repo.Reminders.UpdateReminder(model);
            await Repo.CommitAsync();
            ReminderUpdated?.Invoke(model);

            return model.Text;
        }
        public async Task DeleteReminderAsync(int id)
        {
            await using var Repo = RepositoryFactory.Create();
            if (!await Repo.Reminders.DeleteReminderById(id))
                throw new InvalidMeetIdException();

            await Repo.CommitAsync();
            ReminderDeleted?.Invoke(id);
        }
        
        public async Task<List<ReminderModel>> GetCloseUpcomingRemindersAsync()
        {
            await using var repository = RepositoryFactory.Create();
            return await repository.Reminders.GetNearestReminders();
        }
        public async Task<ReminderModel> GetReminderById(int id)
        {

            await using var Repo = RepositoryFactory.Create();
            return await Repo.Reminders.GetReminderById(id);
        }
    }
}
