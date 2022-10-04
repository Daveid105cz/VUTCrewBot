using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Repository;

namespace VUTCrewBot
{
    public struct ChannelId
    {
        public ChannelId(ulong guild, ulong id)
        {
            this.GuildId = guild;
            this.Id = id;
        }
        public ulong GuildId { get; set; }
        public ulong Id { get; set; }

        public override string ToString()
        {
            return String.Format("{0};{1}", GuildId, Id);
        }
        public static ChannelId? FromString(String value)
        {
            if (String.IsNullOrWhiteSpace(value))
                return null;

            String[] split = value.Split(";");
            if (split.Length != 2)
                return null;
            ulong srvId, channelId = 0;
            if (ulong.TryParse(split[0], out srvId) && ulong.TryParse(split[1], out channelId))
                return new ChannelId(srvId, channelId);
            else
                return null;
        }
    }

    public class BotSettings
    {

        private static readonly String KEY_ADMINNOTIFY_CHANNEL = "AdminNotifyMessagesChannel";
        private static readonly String KEY_MEETNOTIFY_CHANNEL = "MeetsNotifyMessagesChannel";

        IRepositoryFactory factory;
        public BotSettings(IRepositoryFactory repositoryFactory)
        {
            factory = repositoryFactory;
        }
        ChannelId? _botAdminNotifyMessagesChannel;
        public ChannelId? BotAdminNotifyMessagesChannel
        {
            get => _botAdminNotifyMessagesChannel;
            set { _botAdminNotifyMessagesChannel = value; SetChannel(value, KEY_ADMINNOTIFY_CHANNEL); }
        }
        ChannelId? _meetsNotifyChannel;
        public ChannelId? MeetsNotifyChannel
        {
            get => _meetsNotifyChannel;
            set { _meetsNotifyChannel = value; SetChannel(value, KEY_MEETNOTIFY_CHANNEL); }
        }

        public async Task LoadAsync()
        {
            await using var repo = factory.Create();

            _botAdminNotifyMessagesChannel = ChannelId.FromString(await GetValue(repo, KEY_ADMINNOTIFY_CHANNEL, String.Empty));
            _meetsNotifyChannel = ChannelId.FromString(await GetValue(repo, KEY_MEETNOTIFY_CHANNEL, String.Empty));
        }
        private Task<String> GetValue(BotRepository repo, String key, String defaultValue)
        {
            return repo.Settings.GetValueOrDefault(key, String.Empty);
        }
        private void SetChannel(ChannelId? channel, String key)
        {
            if (channel != null)
            {
                Set(key, channel.Value.ToString());
            }
            else
            {
                Set(key, "");
            }
        }
       /* private async Task<Tuple<ulong, ulong>?> LoadAdminNotifyChannel(BotRepository repo)
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
        }*/

        private async void Set(String key, String value)
        {
            await using var repo = factory.Create();
            await repo.Settings.SetValue(key,value);
            await repo.CommitAsync();
        }
    }
}
