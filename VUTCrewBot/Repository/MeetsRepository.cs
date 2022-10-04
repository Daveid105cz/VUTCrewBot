using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using VUTCrewBot.DAL;
using VUTCrewBot.Models;

namespace VUTCrewBot.Repository
{
    public interface IMeetRepository
    {
        public Task<List<MeetModel>> GetAllMeets();
        public Task<MeetModelDetail> GetMeetById(int id);
        public Task<List<MeetModel>> GetAllUpcomingMeets();
        public Task<MeetModel> CreateMeet(MeetModel model);
        public Task<MeetModelDetail> UpdateMeet(MeetModelDetail model);
        public Task DeleteMeet(MeetModelDetail model);
        public Task<bool> DeleteMeetById(int id);
        public Task<List<MeetModel>> GetNearestMeets(TimeSpan? timeToMeetingMax = null);
    }
    public class MeetRepository : RepositoryBase, IMeetRepository
    {
        public MeetRepository(BotDbContext context) : base(context)
        {

        }
        public async Task<MeetModel> CreateMeet(MeetModel model)
        {
            
            var entity = await Context.Meets.AddAsync(model);
            if (entity.Entity != model.Entity)
            {
                return null;
            }
            //await Context.SaveChangesAsync();
            
            return entity.Entity;
        }

        public async Task DeleteMeet(MeetModelDetail model)
        {
            Context.Meets.Remove(model);

        }

        public async Task<bool> DeleteMeetById(int id)
        {
            var entity = Context.Meets.FirstOrDefault(m => m.Id == id);
            if (entity == null)
                return false;
            Context.Meets.Remove(entity);
            return true;
            //await Context.SaveChangesAsync();
        }

        public async Task<List<MeetModel>> GetAllMeets()
        {
            //await using var Context = DbFactory.CreateContext();
            var meets = await Context.Meets
                .ToListAsync();
            return new List<MeetModel>(meets.Select(x => (MeetModel)x));
        }

        public async Task<List<MeetModel>> GetAllUpcomingMeets()
        {
            //await using var Context = DbFactory.CreateContext();
            var meets = await Context.Meets
                .Where(x => x.MeetupTime > DateTime.Now)
                .ToListAsync();
            return new List<MeetModel>(meets.Select(x => (MeetModel)x));
        }

        public async Task<MeetModelDetail> GetMeetById(int id)
        {
            //TODO: include users
            var entity = await Context.Meets
                .Include(x=> x.UserResponses)
                .FirstOrDefaultAsync(x => x.Id == id);
            return entity;
        }

        public async Task<List<MeetModel>> GetNearestMeets(TimeSpan? timeToMeetingMax = null)
        {
            if(timeToMeetingMax==null)
            {
                timeToMeetingMax = new TimeSpan(2, 0, 20); //2h 20seconds
            }

            var now = DateTime.Now;
            var upLimit = DateTime.Now + timeToMeetingMax;

            var meets = await Context.Meets
                .Include(x => x.UserResponses)
                .Where(x => (x.MeetupTime >= now && x.MeetupTime <= upLimit))
                .OrderBy(x => x.MeetupTime)
                .ToListAsync();

            if (meets == null)
                return new List<MeetModel>();
            else
                return new List<MeetModel>(meets.Select(x => (MeetModel)x));
        }

        public async Task<MeetModelDetail> UpdateMeet(MeetModelDetail model)
        {
            Context.Meets.Update(model);
            //await Context.SaveChangesAsync();
            return model;
        }
    }
}
