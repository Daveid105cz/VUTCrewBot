using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.DAL
{
    public class BotDbContext:DbContext
    {
        public BotDbContext(DbContextOptions contextOptions) : base(contextOptions) { }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
