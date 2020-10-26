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
                try
                {
                    a.Invoke();
                }
                catch (Exception e)
                {
                    Utils.Log(Models.LoggerLevel.Error, e.ToString());
                }
            })).Start();
        }

        public static void QueueTimed(Action a, int sec)
        {
            Action handler = null;

            handler = new Action(() =>
            {
                try
                {
                    a.Invoke();
                }
                catch (Exception e)
                {
                    Utils.Log(Models.LoggerLevel.Error, e.ToString());
                }
                Thread.Sleep(sec * 1000);
                new Thread(new ThreadStart(handler)).Start();
            });

            new Thread(new ThreadStart(handler)).Start();
        }
    }
}
