using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.Logging
{

    public class ColorConsoleLogger : ILogger
    {
        private readonly string _name;
        private readonly ColorConsoleLoggerConfiguration _config;

        public ColorConsoleLogger(
            string name,
            ColorConsoleLoggerConfiguration config) =>
            (_name, _config) = (name, config);

        public IDisposable BeginScope<TState>(TState state) => default;

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= _config.LogLevel;
        private static readonly object _lock = new object();
        string TimestampFormat = "yyyy-MM-dd HH:mm:ss";
        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            /*if(logLevel == LogLevel.Debug)
            {
                Console.WriteLine("f----------------------------------------rthrhrth");
            }*/
            if (!IsEnabled(logLevel))
            {
                return;
            }

            lock (_lock)
            {
                var ename = eventId.Name;
                ename = ename?.Length > 12 ? ename?.Substring(0, 12) : ename;
                Console.Write($"[{DateTimeOffset.Now.ToString(this.TimestampFormat)}] [{eventId.Id,-4}/{ename,-12}] ");

                switch (logLevel)
                {
                    case LogLevel.Trace:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;

                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.DarkMagenta;
                        break;

                    case LogLevel.Information:
                        Console.ForegroundColor = ConsoleColor.DarkCyan;
                        break;

                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case LogLevel.Critical:
                        Console.BackgroundColor = ConsoleColor.Red;
                        Console.ForegroundColor = ConsoleColor.Black;
                        break;
                }
                Console.Write(logLevel switch
                {
                    LogLevel.Trace => "[Trace] ",
                    LogLevel.Debug => "[Debug] ",
                    LogLevel.Information => "[Info ] ",
                    LogLevel.Warning => "[Warn ] ",
                    LogLevel.Error => "[Error] ",
                    LogLevel.Critical => "[Crit ]",
                    LogLevel.None => "[None ] ",
                    _ => "[?????] "
                });
                Console.ResetColor();

                //The foreground color is off.
                if (logLevel == LogLevel.Critical)
                    Console.Write(" ");

                var message = formatter(state, exception);
                Console.WriteLine(message);
                if (exception != null)
                    Console.WriteLine(exception);
            }
        }
        
    }
    public sealed class ColorConsoleLoggerProvider : ILoggerProvider
        {
            private readonly ColorConsoleLoggerConfiguration _config;
            private readonly ConcurrentDictionary<string, ColorConsoleLogger> _loggers =
                new ConcurrentDictionary<string, ColorConsoleLogger>();

            public ColorConsoleLoggerProvider(ColorConsoleLoggerConfiguration config) =>
                _config = config;

            public ILogger CreateLogger(string categoryName) =>
                _loggers.GetOrAdd(categoryName, name => new ColorConsoleLogger(name, _config));

            public void Dispose() => _loggers.Clear();
        }
    public static class ColorConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddColorConsoleLogger(
            this ILoggingBuilder builder) =>
            builder.AddColorConsoleLogger(
                new ColorConsoleLoggerConfiguration());

        public static ILoggingBuilder AddColorConsoleLogger(
            this ILoggingBuilder builder,
            Action<ColorConsoleLoggerConfiguration> configure)
        {
            var config = new ColorConsoleLoggerConfiguration();
            configure(config);

            return builder.AddColorConsoleLogger(config);
        }

        public static ILoggingBuilder AddColorConsoleLogger(
            this ILoggingBuilder builder,
            ColorConsoleLoggerConfiguration config)
        {
            builder.AddProvider(new ColorConsoleLoggerProvider(config));
            return builder;
        }
    }
    public class ColorConsoleLoggerConfiguration
    {
        public int EventId { get; set; }
        public LogLevel LogLevel { get; set; } = LogLevel.Trace;
        public ConsoleColor Color { get; set; } = ConsoleColor.Green;
    }
}
