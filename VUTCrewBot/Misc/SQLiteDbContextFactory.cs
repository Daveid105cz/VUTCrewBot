using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;

namespace VUTCrewBot.Misc
{
    public class SQLiteDbContextFactory : IBotDbContextFactory
    {
        private readonly String _connectionString;
        public SQLiteDbContextFactory(BotConfiguration botConfiguration)
        {
            _connectionString = botConfiguration.DbConnectionString;
        }
        public BotDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BotDbContext>();
            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.UseSqlite(_connectionString);
            return new BotDbContext(optionsBuilder.Options);
        }
    }
}
