using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL.Entities
{
    public class MeetTemplateUserEntity
    {
        [Key, Column(Order = 0)]
        public int MeetTemplateId { get; set; }

        [Key, Column(Order = 1)]
        public ulong UserId { get; set; }
    }
}
