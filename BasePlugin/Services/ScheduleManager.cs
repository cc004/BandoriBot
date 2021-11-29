using System;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Services
{
    public static class ScheduleManager
    {
        public static void QueueOnce(Action a, int sec)
        {
            Task.Delay(sec * 1000).ContinueWith(_ =>
            {
                try
                {
                    a.Invoke();
                }
                catch (Exception e)
                {
                    Utils.Log(Models.LoggerLevel.Error, e.ToString());
                }
            }).Start();
        }

        public static void QueueTimed(Func<Task> a, int sec)
        {
            new Thread(new ThreadStart(() =>
            {
                while (true)
                {
                    Thread.Sleep(sec);
                    try
                    {
                        a.Invoke().Wait();
                    }
                    catch (Exception e)
                    {
                        Utils.Log(Models.LoggerLevel.Error, e.ToString());
                    }
                }
            })).Start();
        }
    }
}
