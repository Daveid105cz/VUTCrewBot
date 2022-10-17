using BotToolkit.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;

namespace VUTCrewBot.Repository
{
    public sealed class BotRepository : MainRepositoryBase<BotDbContext>
    {

        public BotRepository(IBotDbContextFactory dbFactory):base(dbFactory) 
        { 

        }


        public IMeetRepository Meet => GetOrCreateRepository<MeetRepository>();
        public IMeetTemplateRepository Templates => GetOrCreateRepository<MeetTemplateRepository>();
        public ISettingsRepository Settings => GetOrCreateRepository<SettingsRepository>();

    }

}
