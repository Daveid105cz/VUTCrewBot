

using Microsoft.EntityFrameworkCore;
using VUTCrewBot.DAL;

namespace VUTCrewBot.Repository
{
    public class RepositoryBase
    {
        protected BotDbContext Context { get; private set; }
        public RepositoryBase(BotDbContext context)
        {
            Context = context;
        }
    }
}
