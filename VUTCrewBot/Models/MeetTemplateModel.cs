using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL.Entities;
using ZbytkyBot.Models;

namespace VUTCrewBot.Models
{
    public class MeetTemplateModel : ModelBase<MeetTemplateEntity>
    {
        public MeetTemplateModel(MeetTemplateEntity entity) : base(entity)
        {

        }
        public MeetTemplateModel() : base(new MeetTemplateEntity())
        {
        }
        public String Name { get => Entity.Name; set => Entity.Name = value; }
        public TimeSpan MeetupDayTime { get => Entity.MeetupDayTime; set => Entity.MeetupDayTime = value; }
        public bool DoGenerate { get => Entity.DoGenerate; set => Entity.DoGenerate = value; }
        public DayOfWeek RepeatDay { get => Entity.RepeatDay; set => Entity.RepeatDay = value; }

        public String GenerateString
        {
            get
            {
                return Helpers.ToAutogenEnum(RepeatDay, DoGenerate).ToString();
            }
        }

        public static implicit operator MeetTemplateModel(MeetTemplateEntity entity)
            => new(entity);
        public static implicit operator MeetTemplateEntity(MeetTemplateModel model)
            => model.Entity;
    }

    public class MeetTemplateModelDetail : MeetTemplateModel
    {
        public MeetTemplateModelDetail(MeetTemplateEntity entity) : base(entity)
        {
            hookCollection();
        }

        public ObservableCollection<MeetTemplateUserEntity> Users { get; set; }

        private void hookCollection()
        {
            if (Entity.Users == null)
            {
                Entity.Users = new List<MeetTemplateUserEntity>();
            }
            Users = new ObservableCollection<MeetTemplateUserEntity>(Entity.Users);
            Users.CollectionChanged += Responses_CollectionChanged;
        }

        private void Responses_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Entity.Users.Clear();
            Entity.Users.AddRange(Users);
        }

        public bool HasUser(ulong id)
        {
            var existingUser = Users.FirstOrDefault(r => r.UserId == id);
            return existingUser != null;
        }
        public MeetTemplateUserEntity GetUser(ulong userId)
        {
            return Users.FirstOrDefault(r => r.UserId == userId);
        }

        public static implicit operator MeetTemplateModelDetail(MeetTemplateEntity entity)
            => new(entity);
        public static implicit operator MeetTemplateEntity(MeetTemplateModelDetail model)
            => model.Entity;
    }
}
