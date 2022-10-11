using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL.Entities
{
    public class MeetEntity:BasicEntity
    {
        public String Name { get; set; }
        public DateTime MeetupTime { get; set; }
        public String? Location { get; set; }
        public List<UserMeetResponse> UserResponses { get; set; }
    }
}
