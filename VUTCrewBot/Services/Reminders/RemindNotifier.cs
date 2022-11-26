using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using VUTCrewBot.Models;

namespace VUTCrewBot.Services.Reminders
{
    public class RemindNotifier:BotService
    {
        public RemindService RemindService { get; set; }
        DiscordChannel _channel = null;
        protected DiscordChannel Channel
        {
            get
            {
                if (_channel == null)
                {
                    _channel = Client.GetChannelAsync(Settings.MeetsNotifyChannel.Value.Id).Result;
                }
                return _channel;
            }
        }


        Dictionary<int, ScheduledMeetObject> scheduledObjects = new Dictionary<int, ScheduledMeetObject>();
        public object dictLocker = new object();


        public RemindNotifier(ILogger<MeetNotifier> logger, RemindService service,
            ServiceContext ctx) : base(logger, ctx)
        {
            RemindService = service;
            RemindService.ReminderCreated += RemindService_RemindCreated;
            RemindService.ReminderUpdated += RemindService_RemindUpdated;
            RemindService.ReminderDeleted += RemindService_RemindDeleted;
        }
        public async override Task Init()
        {
            //RetrieveAndScheduleNearest();
        }

        private void RemindService_RemindCreated(ReminderModel model)
        {
            //RetrieveAndScheduleNearest();
        }
        private void RemindService_RemindUpdated(ReminderModel detail)
        {
            //TryUpdateExistingScheduledMeet(detail);
        }
        private void RemindService_RemindDeleted(int id)
        {
            //TryDeleteScheduledMeet(id);
        }
        /*public async Task RetrieveAndScheduleNearest()
        {
            var meets = await RemindService.GetCloseUpcomingRemindersAsync();

            lock (dictLocker)
            {
                foreach (var meet in meets)
                {
                    if (!scheduledObjects.ContainsKey(meet.Id))
                    {
                        Logger.LogInformation($"A nonregistered meet found {meet.Name} time: {meet.MeetupTime}");
                        var newScheduledObject = new ScheduledMeetObject(meet.Id, meet.MeetupTime, Logger);
                        scheduledObjects.Add(meet.Id, newScheduledObject);
                        newScheduledObject.DisplayIncomingEvent += ScheduledObject_DisplayIncomingEvent;
                        newScheduledObject.MeetEndedEvent += ScheduledObject_MeetEndedEvent;
                        newScheduledObject.Begin();
                    }
                }
            }
        }
        */
    }
}