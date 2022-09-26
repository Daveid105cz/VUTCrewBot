using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VUTCrewBot.DAL;

namespace VUTCrewBot
{
    public class Worker : BackgroundService
    {
        private IHostApplicationLifetime _lifetime;
        private ILogger Logger;
        private BotConfiguration Config;
        private IServiceProvider _provider;
        CrewBot _bot;

        public Worker(IHostApplicationLifetime lifetime, ILogger logger, BotConfiguration botConfiguration, IServiceProvider serviceProvider)
        {
            this._lifetime = lifetime;
            this.Logger = logger;
            this.Config = botConfiguration;
            this._provider = serviceProvider;
        }

        public async override Task StartAsync(CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(Config.DbConnectionString))
            {
                Logger.LogCritical("Database connection string not set. Cannot continue!!");
                _lifetime.StopApplication();
                return;
            }

            var dbFactory = _provider.GetService<IBotDbContextFactory>();
            var context = dbFactory.CreateDbContext();
            await context.Database.MigrateAsync();

            if (String.IsNullOrEmpty(Config.Token))
            {
                Logger.LogCritical("Discord bot token not set. Cannot continue!!");
                _lifetime.StopApplication();
                return;
            }
            _bot = _provider.GetService<CrewBot>();
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            throw new NotImplementedException();
        }
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}
