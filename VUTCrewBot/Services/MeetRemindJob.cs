using BotToolkit.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.Services
{
    public class MeetRemindJob : IMyJob
    {
        MeetNotifier Notifier { get; set; }
        public MeetRemindJob(MeetNotifier notifier)
        {
            Notifier = notifier;
        }
        public async Task RunAsync()
        {
            await Notifier.RetrieveAndScheduleNearest();
        }
    }
}
