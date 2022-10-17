using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL.Entities;
using VUTCrewBot.Models;

namespace VUTCrewBot.Services
{
    public class MeetNotifier: BotService
    {
        public MeetService MeetService { get; set; }
        DiscordChannel _channel = null;
        protected DiscordChannel Channel 
        { 
            get 
            { 
                if(_channel == null)
                {
                    _channel = Client.GetChannelAsync(Settings.MeetsNotifyChannel.Value.Id).Result;
                }
                return _channel; 
            } 
        }


        Dictionary<int, ScheduledMeetObject> scheduledObjects = new Dictionary<int, ScheduledMeetObject>();
        public object dictLocker = new object();


        public MeetNotifier(ILogger<MeetNotifier> logger,  MeetService service, 
            ServiceContext ctx):base(logger, ctx)
        {
            MeetService = service;
            MeetService.MeetCreated += MeetService_MeetCreated;
            MeetService.MeetUpdated += MeetService_MeetUpdated;
            MeetService.MeetDeleted += MeetService_MeetDeleted;
        }
        public async override Task Init()
        {
            RetrieveAndScheduleNearest();
            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
        }

        private void MeetService_MeetCreated(MeetModel model)
        {
            RetrieveAndScheduleNearest();
        }
        private void MeetService_MeetUpdated(MeetModelDetail detail)
        {
            TryUpdateExistingScheduledMeet(detail);
        }
        private void MeetService_MeetDeleted(int id)
        {
            TryDeleteScheduledMeet(id);
        }


        private async Task Client_ComponentInteractionCreated(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            if (e.Channel == Channel)
            {

                ScheduledMeetObject foundMeet = null;
                lock (dictLocker)
                {
                    var existing = scheduledObjects.FirstOrDefault(s => s.Value.InfoMessageId == e.Message.Id,
                        new KeyValuePair<int, ScheduledMeetObject>(-1, null));

                    if (existing.Value != null)
                    {
                        foundMeet = existing.Value;
                    }
                }
                if (foundMeet == null)
                {
                    var builder = new DiscordInteractionResponseBuilder()
                      .WithContent("Na tento sraz se již nelze zapsat")
                      .AsEphemeral();
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                    return;
                }
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                UserResponse response = e.Id.StartsWith("ack:") ? UserResponse.Acked : UserResponse.Refused;
                

                _ = Task.Factory.StartNew(async () =>
                {
                    await MeetService.SetUserResponseAsync(foundMeet.MeetId, e.User.Id, response);
                    Logger.LogInformation($"User {e.User.Username} set reponse to {response}");
                });

            }

        }

        public async Task RetrieveAndScheduleNearest()
        {
            var meets = await MeetService.GetCloseUpcomingMeetsAsync();

            lock (dictLocker)
            {
                foreach (var meet in meets)
                {
                    if (!scheduledObjects.ContainsKey(meet.Id))
                    {
                        Logger.LogInformation($"A nonregistered meet found {meet.Name} time: {meet.MeetupTime}");
                        var newScheduledObject = new ScheduledMeetObject(meet.Id,meet.MeetupTime, Logger);
                        scheduledObjects.Add(meet.Id, newScheduledObject);
                        newScheduledObject.DisplayIncomingEvent += ScheduledObject_DisplayIncomingEvent;
                        newScheduledObject.MeetEndedEvent += ScheduledObject_MeetEndedEvent;
                        newScheduledObject.Begin();
                    }
                }
            }
        }
        public async Task TryUpdateExistingScheduledMeet(MeetModelDetail model)
        {
            bool wasFound = false;
            lock (dictLocker)
            {
                if (scheduledObjects.ContainsKey(model.Id))
                {
                    wasFound = true;
                    ScheduledMeetObject currentMeet = scheduledObjects[model.Id];
                    if(currentMeet.MeetupTime!=model.MeetupTime)
                    {
                        currentMeet.Cancel();
                        currentMeet.SetNewTime(model.MeetupTime);
                        currentMeet.Begin();
                    }
                    SendOrUpdateEmbed(currentMeet);
                }
            }
            if (!wasFound)
            {
                RetrieveAndScheduleNearest();
            }
        }
        public async Task TryDeleteScheduledMeet(int id)
        {
            lock (dictLocker)
            {
                if (scheduledObjects.ContainsKey(id))
                {
                    ScheduledMeetObject currentMeet = scheduledObjects[id];
                    currentMeet.Cancel();
                    scheduledObjects.Remove(id);
                }
            }
        }
        private void ScheduledObject_MeetEndedEvent(ScheduledMeetObject sender)
        {
            SendOrUpdateEmbed(sender);
            lock(dictLocker)
            {
                scheduledObjects.Remove(sender.MeetId);
                Logger.LogInformation($"Clearing id {sender.MeetId} from scheduled meets");
            }
        }

        private async void ScheduledObject_DisplayIncomingEvent(ScheduledMeetObject sender)
        {
            SendOrUpdateEmbed(sender);

        }

        private async Task SendOrUpdateEmbed(ScheduledMeetObject scheduledMeetObject)
        {

            DiscordChannel? toSendTo = Channel;
            if (toSendTo == null)
                return;


            MeetModelDetail detail = await MeetService.GetMeetDetail(scheduledMeetObject.MeetId);

            var embed = await BuildEmbedAsync(detail, !scheduledMeetObject.IsFinished);

            if (scheduledMeetObject.InfoMessageId != 0)
            {
                DiscordMessage message = await toSendTo.GetMessageAsync(scheduledMeetObject.InfoMessageId);
                await message.ModifyAsync(embed);
            }
            else
            {
                var createdMessage = await toSendTo.SendMessageAsync(embed);
                scheduledMeetObject.InfoMessageId = createdMessage.Id;
            }
        }
        private async Task<DiscordMessageBuilder> BuildEmbedAsync(MeetModelDetail e, bool showButtons)
        {
            String time = e.MeetupTime.ToString("HH:mm") + "\n" + e.MeetupTime.ToString("dd.MM.yyyy");
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithTitle("Nadcházející sraz")
                .AddField("Název", e.Name)
                .AddField("Čas", time);
            String users = "";
            String pureMentions = "";
            List<IMention> ments = new List<IMention>();
            if (e.Responses.Count > 0)
            {

                foreach (var user in e.Responses)
                {
                    DiscordUser subedUser = await Client.GetUserAsync(user.UserId);
                    users += String.Format("{0}{1}{2}\n",
                        subedUser.Mention,
                        user.Response != UserResponse.None ? " - " : "",
                        user.Response == UserResponse.Acked
                        ? "Přijde" : (user.Response == UserResponse.Refused ? "Odmítl" : "")
                        );
                    ments.Add(new UserMention(subedUser));
                    pureMentions += subedUser.Mention + ";";
                }
                builder.AddField("Účastníci:", users);

            }


            DiscordMessageBuilder msg = new DiscordMessageBuilder();

            msg.AddEmbed(builder.Build());
            msg.Content = (pureMentions);
            msg.WithAllowedMentions(ments);

            if (showButtons)
            {
                msg.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Success, "ack:", "", emoji: new DiscordComponentEmoji(897136686260166716u)),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "nope:", "", emoji: new DiscordComponentEmoji(828350399047139339u))
                );
            }


            return msg;
        }
    }

    internal class ScheduledMeetObject
    {
        ILogger logger;

        public event Action<ScheduledMeetObject> DisplayIncomingEvent;
        public event Action<ScheduledMeetObject> MeetEndedEvent;

        public int MeetId { get; private set; }
        public DateTime MeetupTime { get; private set; }
        public bool IsFinished { get; private set; }
        public ulong InfoMessageId { get; set; } = 0;

        public ScheduledMeetObject(int meetId, DateTime meetTime, ILogger logger)
        {
            MeetId = meetId;
            MeetupTime = meetTime;
            this.logger = logger;

        }
        CancellationTokenSource source;
        public async void Begin()
        {
            source = new CancellationTokenSource();

            DateTime warnTime = MeetupTime - TimeSpan.FromHours(1);
            logger.LogInformation("Starting wait for meet warn at " + warnTime.ToString());

            await Tasc.WaitUntil(warnTime, source.Token);
            if (source.IsCancellationRequested)
                return;
            DisplayIncomingEvent?.Invoke(this);

            logger.LogInformation("Waiting upon meet finish at " + MeetupTime.ToString());
            

            await Tasc.WaitUntil(MeetupTime, source.Token);
            if (source.IsCancellationRequested)
                return;


            IsFinished = true;
            MeetEndedEvent?.Invoke(this);

        }
        public void Cancel()
        {
            if (source == null)
                throw new InvalidOperationException("CancellationSource is null which means no task is running");
            source.Cancel();
            source.Dispose();
            logger.LogInformation("Canceling waiting for " + MeetId);
        }
        public void SetNewTime(DateTime newMeetTime)
        {
            MeetupTime = newMeetTime;
            logger.LogInformation("Reseting ScheduledObject for " + MeetId);
        }
    }
}
