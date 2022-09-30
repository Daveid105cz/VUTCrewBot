using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL.Entities
{
    public enum UserResponse
    {
        None = 0,
        Acked = 1, 
        Refused = 2
    }
    public class UserMeetResponse
    {
        [Key,Column(Order = 0)]
        public int MeetId { get; set; }

        [Key, Column(Order = 1)]
        public ulong UserId { get; set; }

        public MeetEntity Meet { get; set; }
        public UserResponse Response { get; set; }
    }
}
