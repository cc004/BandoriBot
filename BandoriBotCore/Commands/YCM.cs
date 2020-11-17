using BandoriBot.Config;
using BandoriBot.DataStructures;
using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

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
            "车滚"
        };

        public YCM()
        {
            listener = new StationListener();
            listener.Start();
        }
        public void Run(CommandArgs args)
        {

            if (!string.IsNullOrEmpty(args.Arg)) return;

            List<Car> cars = Configuration.GetConfig<CarTypeConfig>()[args.Source.FromGroup] switch
            {
                CarType.Bandori => listener.Cars,
                CarType.Sekai => CarHandler.Cars,
                _ => null
            };

            string result = "";
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
