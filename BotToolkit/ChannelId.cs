using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotToolkit
{
    public struct ChannelId
    {
        public ChannelId(ulong guild, ulong id)
        {
            GuildId = guild;
            Id = id;
        }
        public ulong GuildId { get; set; }
        public ulong Id { get; set; }

        public override string ToString()
        {
            return string.Format("{0};{1}", GuildId, Id);
        }
        public static ChannelId? FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            string[] split = value.Split(";");
            if (split.Length != 2)
                return null;
            ulong srvId, channelId = 0;
            if (ulong.TryParse(split[0], out srvId) && ulong.TryParse(split[1], out channelId))
                return new ChannelId(srvId, channelId);
            else
                return null;
        }
    }
}
