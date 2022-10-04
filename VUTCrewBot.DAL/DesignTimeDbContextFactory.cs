using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BotDbContext>
    {

        public BotDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<BotDbContext>();
            builder.UseSqlite("Data Source=ZbytkyBot.db");

            return new BotDbContext(builder.Options);
        }
    }
}
