using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VUTCrewBot.DAL.Entities;
using VUTCrewBot.Exceptions;
using VUTCrewBot.Misc;
using VUTCrewBot.Models;
using VUTCrewBot.Repository;

namespace VUTCrewBot.Services
{
    public class MeetService : BotService
    {
        public MeetService(IServiceProvider serviceProvider, IRepositoryFactory repoFactory) : base(serviceProvider,repoFactory)
        {

        }
        Dictionary<int, ScheduledMeetObject> scheduledObjects = new Dictionary<int, ScheduledMeetObject>();
        public object dictLocker = new object();

        private async Task<DiscordChannel> GetChannel()
        {
            return await Client.GetChannelAsync(Settings.MeetsNotifyChannel.Value.Id);
        }

        public async override Task Init()
        {
            RetrieveAndScheduleNearest();
            Client.ComponentInteractionCreated += Client_ComponentInteractionCreated;
            //CronJob();
        }
        private async Task CronJob()
        {
            DateTime now = DateTime.Now;
            DateTime nearestHour = new DateTime(now.Year, now.Month, now.Day, now.Hour, 0, 0).AddHours(1);
            Logger.LogInformation("Scheduling meeting cronjob at "+nearestHour.ToString());
            await Tasc.WaitUntil(nearestHour);
            Logger.LogInformation("Running the CRON job");
            //RUN the job
            RetrieveAndScheduleNearest();
            ClearFinished();
            _ = Task.Factory.StartNew(() =>
            {
                CronJob();
            });

        }
        private async Task Client_ComponentInteractionCreated(DiscordClient sender, DSharpPlus.EventArgs.ComponentInteractionCreateEventArgs e)
        {
            if(e.Channel == await GetChannel())
            {

                ScheduledMeetObject foundMeet = null;
                lock(dictLocker)
                {
                    var existing = scheduledObjects.FirstOrDefault(s => s.Value.InfoMessageId == e.Message.Id,
                        new KeyValuePair<int,ScheduledMeetObject>(-1,null));
                    
                    if(existing.Value!=null)
                    {
                        foundMeet = existing.Value;
                    }
                }
                if(foundMeet==null)
                {
                    var builder = new DiscordInteractionResponseBuilder()
                      .WithContent("Na tento sraz se již nelze zapsat")
                      .AsEphemeral();
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, builder);
                    return;
                }
                await e.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
                UserResponse response = e.Id.StartsWith("ack:") ? UserResponse.Acked : UserResponse.Refused;
                Logger.LogInformation($"User {e.User.Username} setting reponse to {response}");
                
                Task.Factory.StartNew(async () =>
                {
                    Logger.LogInformation($"Setting");

                    await SetUserResponseAsync(foundMeet.Model.Id, e.User.Id,response);
                    Logger.LogInformation($"Response set");

                    Logger.LogInformation($"Response set");
                });

            }
            
        }

        public async Task CreateMeetAsync(String name, DateTime time)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetModel model = await Repo.Meet.CreateMeet(new MeetModel()
            {
                Name = name,
                MeetupTime = time
            });
            await Repo.CommitAsync();
            RefreshMeetScheduled(model.Id);
        }
        public async Task<String> ModifyMeetAsync(int id, String? newName, DateTime? newTime)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetModelDetail model = await Repo.Meet.GetMeetById((int)id);
            if (model == null)
            {
                throw new InvalidMeetIdException();
            }

            model.Name = newName ?? model.Name;
            model.MeetupTime = newTime ?? model.MeetupTime;

            await Repo.Meet.UpdateMeet(model);
            await Repo.CommitAsync();
            RefreshMeetScheduled(model.Id,(newTime!=null) ? model:null);
            return model.Name;
        }
        public async Task DeleteMeetAsync(int id)
        {
            await using var Repo = RepositoryFactory.Create();
            if (!await Repo.Meet.DeleteMeetById(id))
                throw new InvalidMeetIdException();

            await Repo.CommitAsync();
            ProcessMeetDeletion(id);
        }
        public async Task<String> AddUserToMeet(int id, ulong userId)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetModelDetail model = await Repo.Meet.GetMeetById((int)id);
            if (model == null)
            {
                throw new InvalidMeetIdException();
            }

            if (model.HasUser(userId))
            {
                throw new ServiceException("Tento uživatel je ve srazu již přidán");
            }


            model.Responses.Add(new DAL.Entities.UserMeetResponse()
            {
                Response = DAL.Entities.UserResponse.None,
                UserId = userId
            });
            await Repo.Meet.UpdateMeet(model);
            await Repo.CommitAsync();
            RefreshMeetScheduled(model.Id);
            return model.Name;
        }
        public async Task<String> RemoveUserFromMeet(int id, ulong userId)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetModelDetail model = await Repo.Meet.GetMeetById((int)id);
            if (model == null)
            {
                throw new InvalidMeetIdException();
            }

            if (!model.HasUser(userId))
            {
                throw new ServiceException("Tento uživatel se ve srazu nenachází");
            }


            model.Responses.Remove(model.GetUserResponse(userId));
            await Repo.Meet.UpdateMeet(model);
            await Repo.CommitAsync();
            RefreshMeetScheduled(model.Id);
            return model.Name;
        }
        public async Task SetUserResponseAsync(int id, ulong userId, UserResponse response)
        {
            await using var Repo = RepositoryFactory.Create();
            MeetModelDetail model = await Repo.Meet.GetMeetById((int)id);

            if (model == null)
            {
                throw new InvalidMeetIdException();
            }

            if (!model.HasUser(userId))
            {
                model.Responses.Add(new UserMeetResponse() { UserId = userId, Response = response });
            }
            else
            {
                UserMeetResponse fullReponse = model.GetUserResponse(userId);
                fullReponse.Response = response;
            }
            await Repo.CommitAsync();
            RefreshMeetScheduled(model.Id);
        }
        public void ClearFinished()
        {
            lock(dictLocker)
            {
                List<int> ids = new List<int>();
                foreach (var item in scheduledObjects)
                {
                    if (item.Value.IsFinished)
                        ids.Add(item.Key);
                }
                foreach (int id in ids)
                {
                    Logger.LogInformation($"Clearing id {id} from active meet objects");
                    scheduledObjects.Remove(id);
                }
            }

        }
        public async Task RetrieveAndScheduleNearest()
        {
            await using var repository = RepositoryFactory.Create();
            var meets = await repository.Meet.GetNearestMeets();

            lock (dictLocker)
            {
                foreach (var meet in meets)
                {
                    if (!scheduledObjects.ContainsKey(meet.Id))
                    {
                        Logger.LogInformation($"A nonregistered meet found {meet.Name} time: {meet.MeetupTime}");
                        ScheduledMeetObject newScheduledObject = new ScheduledMeetObject(meet, Logger);
                        scheduledObjects.Add(meet.Id, newScheduledObject);
                        newScheduledObject.DisplayIncomingEvent += ScheduledObject_DisplayIncomingEvent;
                        newScheduledObject.MeetEndedEvent += ScheduledObject_MeetEndedEvent;
                        newScheduledObject.Begin();
                    }
                }
            }
        }
        public async Task RefreshMeetScheduled(int meetId, MeetModelDetail forceNewModel = null)
        {
            bool wasFound = false;
            lock(dictLocker)
            {
                if (scheduledObjects.ContainsKey(meetId))
                {
                    wasFound = true;
                    ScheduledMeetObject currentMeet = scheduledObjects[meetId];
                    if (forceNewModel!=null)
                    {
                        currentMeet.Cancel();
                        currentMeet.Reset(forceNewModel);
                        currentMeet.Begin();
                    }
                    SendOrUpdateEmbed(currentMeet);
                }
            }
            if(!wasFound)
            {
                RetrieveAndScheduleNearest();
            }
            /*if(!wasFound && updatedMeet.MeetupTime <= DateTime.Now.AddHours(2))
                RetrieveAndScheduleNearest();*/
        }
        public async Task ProcessMeetDeletion(int id)
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
        private void ScheduledObject_MeetEndedEvent(object? sender, MeetModel e)
        {
            SendOrUpdateEmbed(sender as ScheduledMeetObject);
        }

        private async void ScheduledObject_DisplayIncomingEvent(object? sender, MeetModel e)
        {
            SendOrUpdateEmbed(sender as ScheduledMeetObject);

        }
        private async Task SendOrUpdateEmbed(ScheduledMeetObject scheduledMeetObject)
        {

            DiscordChannel? toSendTo = await GetChannel();
            if (toSendTo == null)
                return;


            await using var Repo = RepositoryFactory.Create();
            MeetModelDetail detail = await Repo.Meet.GetMeetById(scheduledMeetObject.Model.Id);

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
            String time = e.MeetupTime.ToString("HH:mm") +"\n"+e.MeetupTime.ToString("dd.MM.yyyy");
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Gold)
                .WithTitle("Nadcházející sraz")
                .AddField("Název", e.Name)
                .AddField("Čas", time);
            String users = "";
            String pureMentions = "";
            List<IMention> ments = new List<IMention>();
            if(e.Responses.Count>0)
            {
                
                foreach (var user in e.Responses)
                {
                    DiscordUser subedUser = await Client.GetUserAsync(user.UserId);
                    users += String.Format("{0}{1}{2}\n",
                        subedUser.Mention,
                        user.Response != DAL.Entities.UserResponse.None ? " - " : "",
                        user.Response == DAL.Entities.UserResponse.Acked
                        ? "Přijde" : (user.Response == DAL.Entities.UserResponse.Refused ? "Odmítl" : "")
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
            //var ems = await toSendTo.Guild.GetEmojisAsync();
            if(showButtons)
            {
                msg.AddComponents(
                    new DiscordButtonComponent(ButtonStyle.Primary, "ack:", "", emoji: new DiscordComponentEmoji(897136686260166716u)),
                    new DiscordButtonComponent(ButtonStyle.Secondary, "nope:", "", emoji: new DiscordComponentEmoji(828350399047139339u))
                );
            }

            
            return msg  ;
        }


        private class ScheduledMeetObject
        {
            ILogger<Worker> logger;

            public event EventHandler<MeetModel> DisplayIncomingEvent;
            public event EventHandler<MeetModel> MeetEndedEvent;
            public MeetModel Model { get; private set; }
            //public TimedEventScheduler Scheduler { get; private set; }
            public bool IsFinished { get; private set; }
            public ulong InfoMessageId { get; set; } = 0;

            

            public ScheduledMeetObject(MeetModel model, ILogger<Worker> logger)
            {
                this.Model = model;
                this.logger = logger;
                //Scheduler = new TimedEventScheduler();
            }
            CancellationTokenSource source;
            public async Task Begin()
            {
                source = new CancellationTokenSource();
                
                DateTime warnTime = Model.MeetupTime - TimeSpan.FromHours(1);
                logger.LogInformation("Starting wait for meet warn at " + warnTime.ToString());

                await Tasc.WaitUntil(warnTime,source.Token);
                if (source.IsCancellationRequested)
                    return;
                DisplayIncomingEvent?.Invoke(this, Model);

                logger.LogInformation("Waiting upon meet finish at " + Model.MeetupTime.ToString());
                await Tasc.WaitUntil(Model.MeetupTime, source.Token);
                if (source.IsCancellationRequested)
                    return;


                IsFinished = true;
                MeetEndedEvent?.Invoke(this, Model);
                


            }
            public void Cancel()
            {
                if (source == null)
                    throw new InvalidOperationException("CancellationSource is null which means no task is running");
                source.Cancel();
                logger.LogInformation("Canceling waiting for " + Model.Name);
            }
            public void Reset(MeetModel model)
            {
                Model = model;
                logger.LogInformation("Reseting ScheduledObject for " + Model.Name);
            }
        }
    }
    
}
