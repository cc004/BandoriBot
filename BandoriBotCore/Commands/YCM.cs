using BandoriBot.Config;
using BandoriBot.DataStructures;
using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class YCM : ICommand
    {
        private readonly StationListener listener;

        public static event Action<Car> OnNewCar;

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
            listener.OnNewCar += car => OnNewCar?.Invoke(car);
            listener.Start();
        }

        public async Task Run(CommandArgs args)
        {

            if (!string.IsNullOrEmpty(args.Arg)) return;

            var cartype = Configuration.GetConfig<CarTypeConfig>()[args.Source.FromGroup];

            if (cartype == CarType.None)
            {
                await args.Callback("你在的群尚未设置车牌类型！");
                return;
            }

            List<Car> cars = cartype switch
            {
                CarType.Bandori => listener.Cars,
                CarType.Sekai => CarHandler.Cars,
                _ => null
            };

            string result = "";
            HashSet<int> indexes = new HashSet<int>();
            var now = DateTime.Now;

            foreach (Car car in cars)
            {
                if (indexes.Contains(car.index)) continue;
                indexes.Add(car.index);
                result += car.ToString(now) + "\n";
            }
            await args.Callback(string.IsNullOrEmpty(result) ? "myc" : result.Substring(0, result.Length - 1));
        }
    }
}
