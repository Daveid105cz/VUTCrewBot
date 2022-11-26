using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;
using VUTCrewBot.Models;

namespace VUTCrewBot.Repository
{
    public interface IReminderRepository
    {
        public Task<List<ReminderModel>> GetAllReminders();
        public Task<ReminderModel> GetReminderById(int id);
        public Task<List<ReminderModel>> GetAllUpcomingReminders();
        public Task<ReminderModel> CreateReminder(ReminderModel model);
        public Task<ReminderModel> UpdateReminder(ReminderModel model);
        public Task DeleteReminder(ReminderModel model);
        public Task<bool> DeleteReminderById(int id);
        public Task<List<ReminderModel>> GetNearestReminders(TimeSpan? timeToMeetingMax = null);
    }
    public class ReminderRepository : RepositoryBase, IReminderRepository
    {
        public ReminderRepository(BotDbContext context) : base(context)
        {

        }
        public async Task<ReminderModel> CreateReminder(ReminderModel model)
        {

            var entity = await Context.Reminders.AddAsync(model);
            if (entity.Entity != model.Entity)
            {
                return null;
            }

            return entity.Entity;
        }

        public async Task DeleteReminder(ReminderModel model)
        {
            Context.Reminders.Remove(model);

        }

        public async Task<bool> DeleteReminderById(int id)
        {
            var entity = Context.Reminders.FirstOrDefault(m => m.Id == id);
            if (entity == null)
                return false;
            Context.Reminders.Remove(entity);
            return true;
        }

        public async Task<List<ReminderModel>> GetAllReminders()
        {
            var reminders = await Context.Reminders
                .ToListAsync();
            return new List<ReminderModel>(reminders.Select(x => (ReminderModel)x));
        }

        public async Task<List<ReminderModel>> GetAllUpcomingReminders()
        {
            var reminders = await Context.Reminders
                .Where(x => x.RemindTime > DateTime.Now)
                .ToListAsync();
            return new List<ReminderModel>(reminders.Select(x => (ReminderModel)x));
        }

        public async Task<ReminderModel> GetReminderById(int id)
        {
            var entity = await Context.Reminders
                .FirstOrDefaultAsync(x => x.Id == id);
            return entity;
        }

        public async Task<List<ReminderModel>> GetNearestReminders(TimeSpan? timeToRemindMax = null)
        {
            if (timeToRemindMax == null)
            {
                timeToRemindMax = new TimeSpan(2, 0, 20); //2h 20seconds
            }

            var now = DateTime.Now;
            var upLimit = DateTime.Now + timeToRemindMax;

            var reminders = await Context.Reminders
                .Where(x => (x.RemindTime >= now && x.RemindTime <= upLimit))
                .OrderBy(x => x.RemindTime)
                .ToListAsync();

            if (reminders == null)
                return new List<ReminderModel>();
            else
                return new List<ReminderModel>(reminders.Select(x => (ReminderModel)x));
        }

        public async Task<ReminderModel> UpdateReminder(ReminderModel model)
        {
            Context.Reminders.Update(model);
            return model;
        }
    }
}
