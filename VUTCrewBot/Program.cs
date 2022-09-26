using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VUTCrewBot;
using VUTCrewBot.Logging;

Console.WriteLine("Hello, World!");
CreateHostBuilder(args).Build().Run();


static IHostBuilder CreateHostBuilder(string[] args)
{
    //BotConfiguration staticBotConfiguration = BotConfigurationLoader.LoadConfiguration();
    return Host.CreateDefaultBuilder(args)
        .UseSystemd()
        .ConfigureLogging((hostContext, builder) =>
        {
            builder.ClearProviders()
            .AddProvider(
                new ColorConsoleLoggerProvider(
                    new ColorConsoleLoggerConfiguration
                    {
                        LogLevel = LogLevel.Debug
                    }));
        })
        .ConfigureServices((hostContext, services) =>
        {
            BotConfiguration staticConfig = new BotConfiguration();
            services.AddHostedService<Worker>();

            services.AddSingleton<BotConfiguration>(provider =>
            {
                return staticConfig;
            });

            services.AddSingleton<MyLoggerFactory>(provider =>
            {
                return new MyLoggerFactory(provider.GetService<ILogger<Worker>>());
            });
            services.AddSingleton<CrewBot>();
            services.AddSingleton<IBotDbContextFactory, SQLiteDbContextFactory>();

            //Services
            services.AddSingleton<RepostsTrackingService>();
            services.AddSingleton<ResponderService>();
            services.AddSingleton<PinService>();
            services.AddSingleton<VideoCrashDetectorService>();

            //Database providers
            services.AddSingleton<ChannelProvider>();
            services.AddSingleton<ImageHashProvider>();
            services.AddSingleton<SettingsProvider>();
        });
}