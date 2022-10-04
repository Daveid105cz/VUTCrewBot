using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL.Entities
{
    public class MeetTemplateEntity:BasicEntity
    {
        public String Name { get; set; }
        public TimeSpan MeetupDayTime { get; set; }
        public bool DoGenerate { get; set; }
        public DayOfWeek RepeatDay { get; set; }

        public List<MeetTemplateUserEntity> Users { get; set; }
    }
}
