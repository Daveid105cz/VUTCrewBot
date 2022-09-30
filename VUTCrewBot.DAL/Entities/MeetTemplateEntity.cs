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
        public bool DoRepeat { get; set; }
        public DayOfWeek RepeatDay { get; set; }

    }
}
