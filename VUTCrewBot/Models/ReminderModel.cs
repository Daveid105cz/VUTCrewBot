using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL.Entities;
using ZbytkyBot.Models;

namespace VUTCrewBot.Models
{
    public class ReminderModel : ModelBase<ReminderEntity>
    {
        public ReminderModel(ReminderEntity entity) : base(entity)
        {
        }
        public ReminderModel():base(new ReminderEntity())
        {

        }

        public String Text { get => Entity.RemindText; set => Entity.RemindText = value; }
        public DateTime RemindTime { get => Entity.RemindTime; set => Entity.RemindTime = value; }
        public ulong User { get => Entity.UserId; set => Entity.UserId = value; }   

        public static implicit operator ReminderModel(ReminderEntity entity)
            => new(entity);
        public static implicit operator ReminderEntity(ReminderModel model)
            => model.Entity;
    }
}
