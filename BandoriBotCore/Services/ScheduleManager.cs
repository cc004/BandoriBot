using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Services
{
    public static class ScheduleManager
    {
        public static void QueueOnce(Action a, int sec)
        {
            new Thread(new ThreadStart(() =>
            {
                Thread.Sleep(sec * 1000);
                a.Invoke();
            })).Start();
        }

        public static void QueueTimed(Action a, int sec)
        {
            Action handler = null;

            handler = new Action(() =>
            {
                a.Invoke();
                Thread.Sleep(sec * 1000);
                new Thread(new ThreadStart(handler)).Start();
            });

            new Thread(new ThreadStart(handler)).Start();
        }
    }
}
