using System.Collections.ObjectModel;
using System.Threading.Channels;
using VUTCrewBot.DAL.Entities;
using ZbytkyBot.Models;

namespace VUTCrewBot.Models
{
    public class MeetModel : ModelBase<MeetEntity>
    {
        public MeetModel(MeetEntity entity) : base(entity)
        {

        }
        public MeetModel() : base(new MeetEntity())
        {
        }
        public String Name { get => Entity.Name; set => Entity.Name = value; }
        public DateTime MeetupTime { get => Entity.MeetupTime; set => Entity.MeetupTime = value; }

        

        public static implicit operator MeetModel(MeetEntity entity)
            => new(entity);
        public static implicit operator MeetEntity(MeetModel model)
            => model.Entity;
    }

    public class MeetModelDetail : MeetModel
    {
        public MeetModelDetail(MeetEntity entity) : base(entity)
        {
            hookCollection();
        }
        public MeetModelDetail() : base(new MeetEntity())
        {
            hookCollection();
        }
        public ObservableCollection<UserMeetResponse> Responses { get; set; }

        private void hookCollection()
        {
            if(Entity.UserResponses==null)
            {
                Entity.UserResponses = new List<UserMeetResponse>();
            }
            Responses = new ObservableCollection<UserMeetResponse>(Entity.UserResponses.OrderBy(e=>e.UserId));
            Responses.CollectionChanged += Responses_CollectionChanged;
        }

        private void Responses_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Entity.UserResponses.Clear();
            Entity.UserResponses.AddRange(Responses.OrderBy(e=>e.UserId));
        }

        public bool HasUser(ulong id)
        {
            var existingUser = Responses.FirstOrDefault(r => r.UserId == id);
            return existingUser != null;
        }
        public UserMeetResponse GetUserResponse(ulong userId)
        {
            return Responses.FirstOrDefault(r => r.UserId == userId);
        }

        public static implicit operator MeetModelDetail(MeetEntity entity)
            => new(entity);
        public static implicit operator MeetEntity(MeetModelDetail model)
            => model.Entity;
    }
}
