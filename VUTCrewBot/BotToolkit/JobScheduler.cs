using Cronos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VUTCrewBot.BotToolkit
{
    public enum JobRunType
    {
        Periodic,
        AtTime
    }

    public interface IMyJob
    {
        public Task RunAsync();
    }
    public class JobCombo
    {
        private static TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");
        public IMyJob Job { get; set; }
        public string Name { get; set; }
        public CronExpression Cron { get; set; }
        CancellationTokenSource CancelSource { get; set; }
        private ILogger Logger { get; set; }
        public JobCombo(IMyJob job, string name, CronExpression expr, ILogger logger)
        {
            Name = name;
            Job = job;
            Cron = expr;
            Logger = logger;
            CancelSource = new CancellationTokenSource();
        }
        public async void Schedule()
        {
            while (!CancelSource.IsCancellationRequested)
            {
                DateTimeOffset? next = Cron.GetNextOccurrence(DateTimeOffset.UtcNow, timeZone);
                if (next == null)
                {
                    Logger.LogWarning($"A next occurence of job {this} not found. Canceling the job!");
                    return;
                }

                DateTimeOffset now = DateTimeOffset.UtcNow;
                var time = next.Value - now;
                Logger.LogDebug($"Scheduling job {this} for {next:dd.MM.yyyy HH:mm} (in {time:hh':'mm':'ss} )");

                await Task.Delay(time, CancelSource.Token);

                if (CancelSource.IsCancellationRequested)
                    return;
                Logger.LogDebug($"Running job {this}");

                try
                {
                    Job.RunAsync();
                }
                catch(Exception ex)
                {
                    Logger.LogError(ex, $"Exception occured during a scheduled job run of {this}");
                }
            }
        }
        public void Cancel()
        {
            CancelSource.Cancel();
        }
        public override string ToString()
        {
            return $"<{Job.GetType()}>:{Name}:[{Cron.ToString()}]";
        }
    }
    public class JobScheduler
    {
        IServiceProvider _provider;
        public JobScheduler(IServiceProvider provider)
        {
            _provider = provider;
        }

        List<JobCombo> jobs = new List<JobCombo>();

        public void RegisterJob<T>(string name, string cronSetting) where T : IMyJob
        {
            //jobs.Add(job);

            IMyJob? job = _provider.GetService<T>();
            if (job == null)
                throw new ArgumentException("Job type not found in service provider");

            //job.Cron = cronSetting;
            var logger = _provider.GetService<ILogger<T>>();
            jobs.Add(new JobCombo(job, name, CronExpression.Parse(cronSetting), logger));
        }
        public void Work()
        {
            foreach (var job in jobs)
            {
                job.Schedule();
            }
        }
        public void Cancel()
        {
            foreach (var job in jobs)
            {
                job.Cancel();
            }
        }

        public DateTimeOffset? GetNext(string cron)
        {
            //"* * * * *"
            CronExpression expression = CronExpression.Parse(cron);
            TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time");

            DateTimeOffset? next = expression.GetNextOccurrence(DateTimeOffset.UtcNow, timeZone);

            return next;
        }

        //private void GetNextOccurence()
        //{
        //    CronSetting cron = new CronSetting();
        //    myTime next = null;
        //    next.addMinutes(1); //so that next is never now
        //    bool done = false;
        //    while (!done)
        //    {
        //        if (cron.minute != '*' && next.minute != cron.minute)
        //        {
        //            if (next.minute > cron.minute)
        //            {
        //                next.addHours(1);
        //            }
        //            next.minute = cron.minute;
        //        }
        //        if (cron.hour != '*' && next.hour != cron.hour)
        //        {
        //            if (next.hour > cron.hour)
        //            {
        //                next.hour = cron.hour;
        //                next.addDays(1);
        //                next.minute = 0;
        //                continue;
        //            }
        //            next.hour = cron.hour;
        //            next.minute = 0;
        //            continue;
        //        }
        //        if (cron.weekday != '*' && next.weekday != cron.weekday)
        //        {
        //            int deltaDays = cron.weekday - next.weekday; //assume weekday is 0=sun, 1 ... 6=sat
        //            if (deltaDays < 0) { deltaDays += 7; }
        //            next.addDays(deltaDays);
        //            next.hour = 0;
        //            next.minute = 0;
        //            continue;
        //        }
        //        if (cron.day != '*' && next.day != cron.day)
        //        {
        //            if (next.day > cron.day || !next.month.hasDay(cron.day))
        //            {
        //                next.addMonths(1);
        //                next.day = 1; //assume days 1..31
        //                next.hour = 0;
        //                next.minute = 0;
        //                continue;
        //            }
        //            next.day = cron.day;
        //            next.hour = 0;
        //            next.minute = 0;
        //            continue;
        //        }
        //        if (cron.month != '*' && next.month != cron.month)
        //        {
        //            if (next.month > cron.month)
        //            {
        //                next.addMonths(12 - next.month + cron.month);
        //                next.day = 1; //assume days 1..31
        //                next.hour = 0;
        //                next.minute = 0;
        //                continue;
        //            }
        //            next.month = cron.month;
        //            next.day = 1;
        //            next.hour = 0;
        //            next.minute = 0;
        //            continue;
        //        }
        //        done = true;
        //    }   
        //}

    }

}
