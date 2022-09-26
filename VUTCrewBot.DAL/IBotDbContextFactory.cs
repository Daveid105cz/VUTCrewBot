using Microsoft.EntityFrameworkCore;

namespace VUTCrewBot.DAL
{
    public interface IBotDbContextFactory : IDbContextFactory<BotDbContext>
    {

    }
}