using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace BotToolkit.BaseTypes
{
    public class ServiceContext<TRepoFactory, TSettings>
    {
        public IServiceProvider ServiceProvider { get; set; }
        public DiscordClient Client { get; set; }
        public TRepoFactory RepositoryFactory { get; set; }
        public TSettings Settings { get; set; }
        public ServiceContext(IServiceProvider serviceProvider,
            DiscordClient client,
            TRepoFactory repositoryFactory,
            TSettings settings)
        {
            ServiceProvider = serviceProvider;
            Client = client;
            RepositoryFactory = repositoryFactory;
            Settings = settings;
        }
    }
    public class BotServiceBase
    {
        public Task InitInternal()
        {
            return Init();
        }
        public virtual Task Init()
        {
            return Task.CompletedTask;
        }
        public virtual Task DeInit()
        {
            return Task.CompletedTask;
        }
    }
    public class BotServiceBase<TRepoFactory, TRepo, TSettings> : BotServiceBase
        where TRepoFactory : class
        where TSettings : class
        where TRepo : class
    {
        public IServiceProvider ServiceProvider { get; set; }
        public ILogger Logger { get; set; }

        public DiscordClient Client { get; set; }
        public TRepoFactory RepositoryFactory { get; set; }
        protected TSettings Settings { get; set; }


        public BotServiceBase(ILogger logger,  ServiceContext<TRepoFactory, TSettings> ctx)
        {
            Logger = logger;
            ServiceProvider = ctx.ServiceProvider;
            Client = ctx.Client;
            RepositoryFactory = ctx.RepositoryFactory;
            Settings = ctx.Settings;
        }

    }

}
