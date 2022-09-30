using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;
using VUTCrewBot.DAL.Entities;

namespace VUTCrewBot.Repository
{
    public interface ISettingsRepository
    {
        public Task SetValue(String key, String value);
        public Task<String?> GetValue(String key);
        public Task<String> GetValueOrDefault(String key, String defaultValue);

    }
    public class SettingsRepository : RepositoryBase, ISettingsRepository
    {

        //private Dictionary<String, String> _keyValues = new();
        public SettingsRepository(BotDbContext context) : base(context)
        {

        }
        public async Task SetValue(String key, String value)
        {
            KeyValueSettingEntity? entity = await Context.KeyValueSettings.FirstOrDefaultAsync(e => e.Key == key);
            if(entity == null)
            {
                entity = new KeyValueSettingEntity();
                entity.Key = key;
                entity.Value = value;
                Context.KeyValueSettings.Add(entity);
            }
            else
            {
                entity.Value = value;
            }
        }
        public async Task<String?> GetValue(String key)
        {
            KeyValueSettingEntity? entity = await Context.KeyValueSettings.FirstOrDefaultAsync(e => e.Key == key);
            if (entity == null)
            {
                return null;
            }
            else
            {
                return entity.Value;
            }
        }

        public async Task<string> GetValueOrDefault(string key, string defaultValue)
        {
            KeyValueSettingEntity? entity = await Context.KeyValueSettings.FirstOrDefaultAsync(e => e.Key == key);
            if (entity == null)
            {
                return defaultValue;
            }
            else
            {
                return entity.Value;
            }
        }
    }
}
