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
        public delegate void MeetUpdatedEventHandler(MeetModelDetail detail);
        public delegate void MeetDeletedEventHandler(int id);
        public delegate void MeetCreatedEventHandler(MeetModelDetail model);

        public event MeetUpdatedEventHandler MeetUpdated;
        public event MeetDeletedEventHandler MeetDeleted;
        public event MeetCreatedEventHandler MeetCreated;

        public MeetService(ILogger<MeetService> logger,  ServiceContext ctx) : base(logger, ctx)
        {

        }


        public async Task CreateMeetAsync(String name, DateTime time, List<ulong> users = null)
        {
            await using var Repo = RepositoryFactory.Create();
            var newModel = new MeetModelDetail()
            {
                Name = name,
                MeetupTime = time
            };

            if(users!=null)
            {
                foreach (ulong user in users)
                {
                    newModel.Responses.Add(new UserMeetResponse()
                    {
                        UserId = user,
                        Response = UserResponse.None
                    });
                }
            }

            await Repo.Meet.CreateMeet(newModel);
            await Repo.CommitAsync();
            MeetCreated?.Invoke(newModel);
            //RefreshMeetScheduled(model.Id);
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
            MeetUpdated?.Invoke(model);
            //RefreshMeetScheduled(model.Id,(newTime!=null) ? model:null);
            return model.Name;
        }
        public async Task DeleteMeetAsync(int id)
        {
            await using var Repo = RepositoryFactory.Create();
            if (!await Repo.Meet.DeleteMeetById(id))
                throw new InvalidMeetIdException();

            await Repo.CommitAsync();
            MeetDeleted?.Invoke(id);
            //ProcessMeetDeletion(id);
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
            MeetUpdated?.Invoke(model);
            //RefreshMeetScheduled(model.Id);
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
            MeetUpdated?.Invoke(model);
            //RefreshMeetScheduled(model.Id);
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
            MeetUpdated?.Invoke(model);
        }
        public async Task<List<MeetModel>> GetCloseUpcomingMeetsAsync()
        {
            await using var repository = RepositoryFactory.Create();
            return await repository.Meet.GetNearestMeets();
        }
        public async Task<MeetModelDetail> GetMeetDetail(int id)
        {

            await using var Repo = RepositoryFactory.Create();
            return await Repo.Meet.GetMeetById(id);
        }
    }
    
}
