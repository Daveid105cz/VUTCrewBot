using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotToolkit.Repositories
{
    public class RepositoryBase<TDbContext> where TDbContext : DbContext
    {
        protected TDbContext Context { get; private set; }
        public RepositoryBase(TDbContext context)
        {
            Context = context;
        }
    }
}
