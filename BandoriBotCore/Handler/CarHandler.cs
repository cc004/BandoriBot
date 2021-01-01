using BandoriBot.Config;
using BandoriBot.DataStructures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BandoriBot.Handler
{
    public class CarHandler : IMessageHandler
    {
        private static string source = HttpUtility.UrlEncode("冲冲");
        private static string token = "2NmWeiklE";
        private static List<Car> sekaicars = new List<Car>();

        public bool IgnoreCommandHandled => false;

        public static List<Car> Cars
        {
            get
            {
                var nt = DateTime.Now;
                lock (sekaicars)
                {
                    sekaicars = sekaicars.Where(car => nt - car.time <= (car.index > 99999 ? new TimeSpan(0, 10, 0) : new TimeSpan(0, 2, 0)))
                        .OrderByDescending(c => c.time).ToList();
                    return sekaicars;
                }
            }
        }

        private bool IsIgnore(Source sender)
        {
            if (!Configuration.GetConfig<Activation>()[sender.FromGroup]) return true;
            if ((sender.FromGroup > 0) && !Configuration.GetConfig<Activation>()[sender.FromQQ]) return true;
            return false;
        }

        private static bool IsNumeric(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static readonly Dictionary<long, int> LastCar = new Dictionary<long, int>();

        private static readonly Regex codereg = new Regex(@"\[.*?\]", RegexOptions.Compiled);

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            var message = args.message;

            if (message == "m" || message == "M")
            {
                if (LastCar.TryGetValue(args.Sender.FromQQ, out int lc))
                    lock (sekaicars)
                        sekaicars = sekaicars.Where(c => c.index != lc).ToList();
                return true;
            }

            int split, car;
            if (IsIgnore(args.Sender)) return false;
            if (message.Length < 5) return false;
            for (int i = 0; i < 5; ++i)
                if (!IsNumeric(message[i])) return false;
            split = message.Length > 5 && IsNumeric(message[5]) ? 6 : 5;
            if (message.Length > split && IsNumeric(message[split])) return false;

            car = int.Parse(message.Substring(0, split));
            LastCar[args.Sender.FromQQ] = car;

            await Task.Delay(Configuration.GetConfig<Delay>()[args.Sender.FromQQ] * 1000);

            string raw_message = car.ToString("d5") + " " + message.Substring(split);

            // ignore non-text messages

            raw_message = codereg.Replace(raw_message, _ => "");

            switch (Configuration.GetConfig<CarTypeConfig>()[args.Sender.FromGroup])
            {

                case CarType.Bandori:
                    JObject res = await Utils.GetHttp($"http://api.bandoristation.com/?function=submit_room_number&number={car}&source={source}&token={token}&raw_message={raw_message}&user_id={args.Sender.FromQQ}");
                    if (res == null)
                    {
                        await args.Callback($"无法连接到bandoristation.com");
                    }
                    else if (res["status"].ToString() != "success" && res["status"].ToString() != "duplicate_number_submit")
                    {
                        await args.Callback($"上传车牌时发生错误: {res["status"]}");
                    }
                    return true;
                case CarType.Sekai:
                    lock (sekaicars)
                        sekaicars.Add(new Car
                        {
                            index = car,
                            rawmessage = raw_message,
                            time = DateTime.Now
                        });
                    return true;
            }

            return false;
        }
    }
}
