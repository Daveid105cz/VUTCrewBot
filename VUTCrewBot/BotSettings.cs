using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Repository;

namespace VUTCrewBot
{
    public class BotSettings
    {
        IRepositoryFactory factory;
        public BotSettings(IRepositoryFactory repositoryFactory)
        {
            factory = repositoryFactory;
        }
        Tuple<ulong, ulong>? _botAdminNotifyMessagesChannel;
        public Tuple<ulong, ulong>? BotAdminNotifyMessagesChannel
        {
            get
            {
                return _botAdminNotifyMessagesChannel;
                
            }
            set
            {
                _botAdminNotifyMessagesChannel = value;
                if(value != null)
                {
                    Set("AdminNotifyMessagesChannel", String.Format("{0};{1}", value.Item1, value.Item2));
                }
                else
                {
                    Set("AdminNotifyMessagesChannel", "");
                }

                //_keyValues["AdminNotifyMessagesChannel"] = 
            }
        }
        public async Task LoadAsync()
        {
            await using var repo = factory.Create();

            _botAdminNotifyMessagesChannel = await LoadAdminNotifyChannel(repo)!;
        }
        private async Task<Tuple<ulong, ulong>?> LoadAdminNotifyChannel(BotRepository repo)
        {
            String value = await repo.Settings.GetValueOrDefault("AdminNotifyMessagesChannel", String.Empty);
            if (String.IsNullOrWhiteSpace(value))
                return null;

            String[] split = value.Split(";");
            if (split.Length != 2)
                return null;
            ulong srvId, channelId = 0;
            if (ulong.TryParse(split[0], out srvId) && ulong.TryParse(split[1], out channelId))
                return new Tuple<ulong, ulong>(srvId, channelId);
            else
                return null;
        }

        private async void Set(String key, String value)
        {
            await using var repo = factory.Create();
            await repo.Settings.SetValue(key,value);
            await repo.CommitAsync();
        }
    }
}
