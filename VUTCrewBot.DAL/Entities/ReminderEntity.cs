using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL.Entities
{
    public class ReminderEntity:BasicEntity
    {
        public ulong UserId { get; set; }
        public ulong ChannelId { get; set; }
        public String RemindText { get; set; }
        public DateTime RemindTime { get; set; }
    }
}
