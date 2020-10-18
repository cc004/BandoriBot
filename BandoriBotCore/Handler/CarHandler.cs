using BandoriBot.Config;
using BandoriBot.DataStructures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace BandoriBot.Handler
{
    public class CarHandler : IMessageHandler
    {
        private static string source = HttpUtility.UrlEncode("冲冲");
        private static string token = "2NmWeiklE";
        private static Queue<Car> sekaicars = new Queue<Car>();

        public static List<Car> Cars
        {
            get
            {
                lock (sekaicars)
                {
                    var nt = DateTime.Now;
                    while (sekaicars.Count > 0)
                    {
                        var car = sekaicars.Peek();
                        if (nt - car.time > (car.index > 99999 ? new TimeSpan(0, 10, 0) : new TimeSpan(0, 2, 0)))
                            sekaicars.Dequeue();
                        else
                            break;
                    }

                    return sekaicars.Reverse().ToList();
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

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            if (message == "m" || message == "M")
            {
                if (LastCar.TryGetValue(Sender.FromQQ, out int lc))
                    lock (sekaicars)
                        sekaicars = new Queue<Car>(sekaicars.Where(c => c.index != lc));
                return true;
            }

            int split, car;
            if (IsIgnore(Sender)) return false;
            if (message.Length < 5) return false;
            for (int i = 0; i < 5; ++i)
                if (!IsNumeric(message[i])) return false;
            split = message.Length > 5 && IsNumeric(message[5]) ? 6 : 5;
            if (message.Length > split && IsNumeric(message[split])) return false;

            car = int.Parse(message.Substring(0, split));
            LastCar[Sender.FromQQ] = car;

            Thread.Sleep(Configuration.GetConfig<Delay>()[Sender.FromQQ] * 1000);

            string raw_message = car.ToString("d5") + " " + message.Substring(split);

            // ignore non-text messages

            raw_message = codereg.Replace(raw_message, _ => "");

            switch (Configuration.GetConfig<CarTypeConfig>()[Sender.FromGroup])
            {

                case CarType.Bandori:
                    JObject res = Utils.GetHttp($"http://api.bandoristation.com/?function=submit_room_number&number={car}&source={source}&token={token}&raw_message={raw_message}&user_id={Sender.FromQQ}");
                    if (res == null)
                    {
                        callback($"无法连接到bandoristation.com");
                    }
                    else if (res["status"].ToString() != "success" && res["status"].ToString() != "duplicate_number_submit")
                    {
                        callback($"上传车牌时发生错误: {res["status"]}");
                    }
                    return true;
                case CarType.Sekai:
                    lock (sekaicars)
                        sekaicars.Enqueue(new Car
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
