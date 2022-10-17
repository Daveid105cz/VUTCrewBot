using BotToolkit.BaseTypes;
using BotToolkit.Repositories;
using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;
using VUTCrewBot.Repository;

namespace VUTCrewBot
{
    public interface IRepositoryFactory : IRepositoryFactory<BotRepository, BotDbContext>
    {

    }
    public class RepositoryFactory : IRepositoryFactory
    {
        IServiceProvider provider;
        public RepositoryFactory(IServiceProvider provider)
        {
            this.provider = provider;
        }
        public BotRepository Create()
        {
            return provider.GetService<BotRepository>();
        }
    }
    public class RepositoryBase : RepositoryBase<BotDbContext>
    {
        public RepositoryBase(BotDbContext db) : base(db)
        {

        }
    }
    public class ServiceContext : ServiceContext<IRepositoryFactory, BotSettings>
    {
        public ServiceContext(IServiceProvider serviceProvider,
            DiscordClient client, 
            IRepositoryFactory repositoryFactory, 
            BotSettings settings) :base(serviceProvider, client, repositoryFactory, settings) 
        {
            ServiceProvider = serviceProvider;
            Client = client;
            RepositoryFactory = repositoryFactory;
            Settings = settings;
        }
    }

    public class BotService: BotServiceBase<IRepositoryFactory, BotRepository, BotSettings>
    {
        public BotService(ILogger logger, ServiceContext sc) :base(logger, sc)
        {
            
        }
    }


}
