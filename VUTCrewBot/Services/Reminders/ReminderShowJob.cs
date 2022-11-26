using BotToolkit.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.Services.Reminders
{
    public class ReminderShowJob : IMyJob
    {
        public RemindNotifier Notifier { get; set; }

        public ReminderShowJob(RemindNotifier notifier)
        {
            Notifier = notifier;
        }
        public async Task RunAsync()
        {
            await Notifier.RetrieveAndScheduleNearest();
        }
    }
}
