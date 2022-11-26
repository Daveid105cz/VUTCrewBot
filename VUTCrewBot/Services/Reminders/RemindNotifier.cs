using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System.Net.NetworkInformation;
using System.Reflection;
using VUTCrewBot.Models;

namespace VUTCrewBot.Services.Reminders
{
    public class RemindNotifier:BotService
    {
        public RemindService RemindService { get; set; }
       /* DiscordChannel _channel = null;
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
        }*/


        Dictionary<int, ScheduledReminderObject> scheduledObjects = new Dictionary<int, ScheduledReminderObject>();
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
            RetrieveAndScheduleNearest();
        }

        private void RemindService_RemindCreated(ReminderModel model)
        {
            RetrieveAndScheduleNearest();
        }
        private void RemindService_RemindUpdated(ReminderModel detail)
        {
            TryUpdateExistingScheduledReminder(detail);
        }
        private void RemindService_RemindDeleted(int id)
        {
            TryDeleteScheduledReminder(id);
        }
        public async Task RetrieveAndScheduleNearest()
        {
            var reminders = await RemindService.GetCloseUpcomingRemindersAsync();

            lock (dictLocker)
            {
                foreach (var reminder in reminders)
                {
                    if (!scheduledObjects.ContainsKey(reminder.Id))
                    {
                        Logger.LogInformation($"A nonregistered reminder found {reminder.Text} time: {reminder.RemindTime}");
                        var newScheduledObject = new ScheduledReminderObject(reminder.Id, reminder.RemindTime, Logger);
                        scheduledObjects.Add(reminder.Id, newScheduledObject);
                        newScheduledObject.DisplayReminderEvent += NewScheduledObject_DisplayReminderEvent;
                        newScheduledObject.Begin();
                    }
                }
            }
        }
        public async Task TryUpdateExistingScheduledReminder(ReminderModel model)
        {
            bool wasFound = false;
            lock (dictLocker)
            {
                if (scheduledObjects.ContainsKey(model.Id))
                {
                    wasFound = true;
                    ScheduledReminderObject currentReminder = scheduledObjects[model.Id];
                    if (currentReminder.RemindTime != model.RemindTime)
                    {
                        currentReminder.Cancel();
                        currentReminder.SetNewTime(model.RemindTime);
                        currentReminder.Begin();
                    }
                }
            }
            if (!wasFound)
            {
                RetrieveAndScheduleNearest();
            }
        }
        public async Task TryDeleteScheduledReminder(int id)
        {
            lock (dictLocker)
            {
                if (scheduledObjects.ContainsKey(id))
                {
                    ScheduledReminderObject currentReminder = scheduledObjects[id];
                    currentReminder.Cancel();
                    scheduledObjects.Remove(id);
                }
            }
        }
        private void NewScheduledObject_DisplayReminderEvent(ScheduledReminderObject sender)
        {
            SendReminder(sender.ReminderId);
            lock (dictLocker)
            {
                scheduledObjects.Remove(sender.ReminderId);
                Logger.LogInformation($"Clearing id {sender.ReminderId} from scheduled reminders");
            }
        }
        private async void SendReminder(int reminderId)
        {
            ReminderModel detail = await RemindService.GetReminderById(reminderId);
            DiscordChannel channelToSend = await Client.GetChannelAsync(detail.Channel);
            if(channelToSend==null)
            {
                Logger.LogError($"Channel with id {detail.Channel} not found. Cannot send reminder!");
                return;
            }
            DiscordUser user = await Client.GetUserAsync(detail.User);
            if (channelToSend == null)
            {
                Logger.LogError($"User with id {detail.User} not found. Cannot send reminder!");
                return;
            }
            await channelToSend.SendMessageAsync(
                $"{Formatter.Italic("Pingy pongy, hele hele")} {user.Mention}.\n" +
                $"{Formatter.Italic("Přípomínka:")}\n" +
                $"{Formatter.Bold(detail.Text)}");
        }
    }
    internal class ScheduledReminderObject
    {
        ILogger logger;

        public event Action<ScheduledReminderObject> DisplayReminderEvent;

        public int ReminderId { get; private set; }
        public DateTime RemindTime { get; private set; }

        public ScheduledReminderObject(int reminderId, DateTime remindTime, ILogger logger)
        {
            ReminderId = reminderId;
            RemindTime = remindTime;
            this.logger = logger;

        }
        CancellationTokenSource source;
        public async void Begin()
        {
            source = new CancellationTokenSource();

            logger.LogInformation("Starting wait for reminder show at " + RemindTime.ToString());

            await Tasc.WaitUntil(RemindTime, source.Token);
            if (source.IsCancellationRequested)
                return;
            DisplayReminderEvent?.Invoke(this);

        }
        public void Cancel()
        {
            if (source == null)
                throw new InvalidOperationException("CancellationSource is null which means no task is running");
            source.Cancel();
            source.Dispose();
            logger.LogInformation("Canceling waiting for " + ReminderId);
        }
        public void SetNewTime(DateTime remindTime)
        {
            RemindTime = remindTime;
            logger.LogInformation("Reseting ScheduledRemindObject for " + ReminderId);
        }
    }
}