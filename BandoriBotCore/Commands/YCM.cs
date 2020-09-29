using BandoriBot.DataStructures;
using BandoriBot.Handler;
using System;
using System.Collections.Generic;

namespace BandoriBot.Commands
{
    public class YCM : ICommand
    {
        private StationListener listener;
        public List<string> Alias => new List<string>
        {
            "ycm",
            "有车吗",
            "车来",
            "车",
            "来车",
            "车滚",
            "贴贴"
        };

        public YCM()
        {
            listener = new StationListener();
        }
        public void Run(CommandArgs args)
        {
            if (args.Arg == "#" && args.IsAdmin)
            {
                args.Callback(
                    "Listener status\n" +
                    $"IsRunning : {listener.Running}\n" +
                    $"IsActive : {listener.Active}\n" +
                    $"Cars : {listener.Cars.Count}\n" +
                    $"");
            }
            if (!string.IsNullOrEmpty(args.Arg)) return;
            lock (listener)
                if (!listener.Running)
                    listener.Start();

            string result = "";
            List<Car> cars = listener.Cars;
            HashSet<int> indexes = new HashSet<int>();
            foreach (Car car in cars)
            {
                if (indexes.Contains(car.index)) continue;
                indexes.Add(car.index);
                result +=
                    car.rawmessage +
                    $"({(int)(DateTime.Now - car.time).TotalSeconds}秒前)" +
                    "\n";
            }
            args.Callback(string.IsNullOrEmpty(result) ? "myc" : result.Substring(0, result.Length - 1));
        }
    }
}
