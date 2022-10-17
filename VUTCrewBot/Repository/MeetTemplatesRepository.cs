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
    public interface IMeetTemplateRepository
    {
        public Task<List<MeetTemplateModel>> GetAllTemplates();
        public Task<MeetTemplateModelDetail> GetTemplateById(int id);
        public Task<MeetTemplateModel> CreateTemplate(MeetTemplateModel model);
        public Task<MeetTemplateModelDetail> UpdateTemplate(MeetTemplateModelDetail model);
        public Task DeleteTemplate(MeetTemplateModelDetail model);
        public Task<bool> DeleteTemplate(int id);
        public Task<List<MeetTemplateModelDetail>> GetGeneratedTemplatesOnDay(DayOfWeek day);
    }
    public class MeetTemplateRepository : RepositoryBase, IMeetTemplateRepository
    {
        public MeetTemplateRepository(BotDbContext context) : base(context)
        {

        }

        public async Task<MeetTemplateModel> CreateTemplate(MeetTemplateModel model)
        {
            var entity = await Context.MeetTemplates.AddAsync(model);
            if (entity.Entity != model.Entity)
            {
                return null;
            }

            return entity.Entity;
        }

        public async Task DeleteTemplate(MeetTemplateModelDetail model)
        {
            Context.MeetTemplates.Remove(model);
        }

        public async Task<bool> DeleteTemplate(int id)
        {
            var entity = Context.MeetTemplates.FirstOrDefault(m => m.Id == id);
            if (entity == null)
                return false;
            Context.MeetTemplates.Remove(entity);
            return true;
        }

        public async Task<List<MeetTemplateModel>> GetAllTemplates()
        {
            var meets = await Context.MeetTemplates
                .ToListAsync();
            return new List<MeetTemplateModel>(meets.Select(x => (MeetTemplateModel)x));
        }

        public async Task<MeetTemplateModelDetail> GetTemplateById(int id)
        {
            var entity = await Context.MeetTemplates
                .Include(x => x.Users)
                .FirstOrDefaultAsync(x => x.Id == id);
            return entity;
        }

        public async Task<List<MeetTemplateModelDetail>> GetGeneratedTemplatesOnDay(DayOfWeek day)
        {
            var meets = await Context.MeetTemplates
                .Include(x => x.Users)
                .Where(x => x.RepeatDay == day && x.DoGenerate)
                .ToListAsync();
            return new List<MeetTemplateModelDetail>(meets.Select(x => (MeetTemplateModelDetail)x));
        }

        public async Task<MeetTemplateModelDetail> UpdateTemplate(MeetTemplateModelDetail model)
        {
            Context.MeetTemplates.Update(model);
            return model;
        }
      
    }
}
