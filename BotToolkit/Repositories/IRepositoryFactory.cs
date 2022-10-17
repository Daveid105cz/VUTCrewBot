using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotToolkit.Repositories
{
    public interface IRepositoryFactory<TRepo, TDbContext> where TRepo : MainRepositoryBase<TDbContext> where TDbContext : DbContext
    {
        public TRepo Create();
    }
}
