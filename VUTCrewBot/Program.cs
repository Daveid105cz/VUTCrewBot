using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VUTCrewBot;
using VUTCrewBot.Commands;
using VUTCrewBot.DAL;
using VUTCrewBot.Logging;
using VUTCrewBot.Misc;
using VUTCrewBot.Repository;
using VUTCrewBot.Services;

#if DEBUG
Console.WriteLine("Running in debug mode");
#else
Console.WriteLine("Running in release mode");
#endif

AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

CreateHostBuilder(args).Build().Run();


void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Console.WriteLine(e);
}

static IHostBuilder CreateHostBuilder(string[] args)
{
    BotConfiguration staticConfig = BotConfigurationLoader.LoadConfiguration();
    return Host.CreateDefaultBuilder(args)
        .ConfigureLogging((hostContext, builder) =>
        {
            builder.ClearProviders()
            .AddProvider(
                new ColorConsoleLoggerProvider(
                    new ColorConsoleLoggerConfiguration
                    {
                        LogLevel = staticConfig.LogLevel
                    }));
        })
        .ConfigureServices((hostContext, services) =>
        {
#if DEBUG
            staticConfig.Token = "ODQ3NTYyMTc0MjQyMjI2MjE3.YK_3yQ.doc3AGwQNkJBK92gNeKArAoluqU";
            staticConfig.DbConnectionString = "Data Source=CrewBot.db";
#endif
            services.AddHostedService<Worker>();

            services.AddSingleton<BotConfiguration>(provider =>
            {
                return staticConfig;
            });
            services.AddSingleton<DiscordBotLibLoggerFactory>(provider =>
            {
                return new DiscordBotLibLoggerFactory(provider.GetService<ILogger<DiscordClient>>());
            });
            services.AddSingleton<BotSettings>();
            services.AddSingleton<CrewBot>();
            services.AddSingleton<IBotDbContextFactory, SQLiteDbContextFactory>();
            services.AddTransient<BotRepository>();
            services.AddSingleton<IRepositoryFactory, RepositoryFactory>();

            //Services
            services.AddSingleton<MeetService>();
            services.AddSingleton<TemplateService>();

            //Autocomplete choice providers
            services.AddSingleton<ActiveMeetChoiceProvider>();
            services.AddSingleton<MeetTemplatesChoiceProvider>();
        });
}