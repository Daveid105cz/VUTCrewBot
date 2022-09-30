using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.Misc;
using VUTCrewBot.Repository;

namespace VUTCrewBot.Services
{
    public class BotService
    {
        protected IServiceProvider ServiceProvider { get; set; }
        protected DiscordClient Client { get; set; }
        public ILogger<Worker> Logger { get; set; }
        public IRepositoryFactory RepositoryFactory { get; set; }
        protected BotSettings Settings { get; set; }
        public BotService(IServiceProvider serviceProvider, IRepositoryFactory repositoryFactory)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetService<ILogger<Worker>>();
            RepositoryFactory = repositoryFactory;
            Settings = serviceProvider.GetService<BotSettings>();
        }
        public Task Init(DiscordClient client)
        {
            Client = client;
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

}
