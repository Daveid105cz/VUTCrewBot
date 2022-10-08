using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using VUTCrewBot;
using VUTCrewBot.BotToolkit;
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

/*JobScheduler j = new JobScheduler();
DateTimeOffset nn = DateTimeOffset.UtcNow;
DateTimeOffset? dd =  j.GetNext("0 23 * * *");

var pp = dd.Value - nn;*/

CreateHostBuilder(args).Build().Run();


void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
{
    Console.WriteLine(e);
}

static IHostBuilder CreateHostBuilder(string[] args)
{
    BotConfiguration staticConfig = BotConfigurationLoader.LoadConfiguration();
    staticConfig.LogLevel = LogLevel.Debug;
    return Host.CreateDefaultBuilder(args)
        .ConfigureLogging(loggin =>
        {
            loggin.ClearProviders();
            loggin.SetMinimumLevel(staticConfig.LogLevel);
            loggin.AddSimpleConsole(config =>
            {
                config.SingleLine = true;
                config.ColorBehavior = LoggerColorBehavior.Enabled;
                string timestampFormat = "yyyy-MM-dd HH:mm:ss ";
                config.IncludeScopes = false;
                
                config.TimestampFormat = timestampFormat;
                
            });
        })
        //.ConfigureLogging((hostContext, builder) =>
        //{
        //    builder.ClearProviders()
        //    .AddProvider(
        //        new ColorConsoleLoggerProvider(
        //            new ColorConsoleLoggerConfiguration
        //            {
        //                LogLevel = staticConfig.LogLevel
        //            }));
        //})
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
            services.AddSingleton<JobScheduler>();

            //Services
            services.RegisterAllServices(typeof(CrewBot).Assembly);
            //services.AddSingleton<MeetService>();
            //services.AddSingleton<TemplateService>();

            //Autocomplete choice providers
            services.AddSingleton<ActiveMeetChoiceProvider>();
            services.AddSingleton<MeetTemplatesChoiceProvider>();

            //Jobs
            services.RegisterAllJobs(typeof(CrewBot).Assembly);
            //services.AddTransient<MeetRemindJob>();
            //services.AddTransient<TemplateGenerationJob>();
        });
}