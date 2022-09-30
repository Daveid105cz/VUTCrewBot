using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VUTCrewBot.Misc
{
    public class TimedEventScheduler
    {
        Timer? timer;
        public TimedEventScheduler()
        {
            
            
        }
        Action action;
        private void TimerTick(object? state)
        {
            Stop();
            runCallback();
        }
        private void runCallback()
        {
            _ = Task.Factory.StartNew(() => 
            {
                action?.Invoke();
            });
        }
        object locker = new object();
        public void RunAt(DateTime date, Action callback)
        {
            lock(locker) 
            {
                action = callback;
                if (date <= DateTime.Now)
                {
                    runCallback();
                    return;
                }
                timer = new Timer(TimerTick, null, date - DateTime.Now, Timeout.InfiniteTimeSpan);
            }
        }
        public void Stop()
        {
            lock(locker)
            {
                action = null;
                if(timer!=null)
                {
                    timer.Dispose();
                    timer = null;
                }
            }
        }

    }
}
