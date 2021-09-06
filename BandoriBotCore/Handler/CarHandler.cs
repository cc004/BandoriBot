using BandoriBot.Config;
using BandoriBot.DataStructures;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace BandoriBot.Handler
{
    public class CarHandler : IMessageHandler
    {
        private static readonly string source = HttpUtility.UrlEncode("喵喵喵");
        private static readonly string token = "oYaAqHVn63";
        private static List<Car> sekaicars = new List<Car>();

        public static event Action<Car> OnNewCar;

        public bool IgnoreCommandHandled => false;

        public static List<Car> Cars
        {
            get
            {
                var nt = DateTime.UtcNow;
                lock (sekaicars)
                {
                    sekaicars = sekaicars.Where(car => nt - car.time <= (car.index > 99999 ? new TimeSpan(0, 10, 0) : new TimeSpan(0, 4, 0)))
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

        private static string CheckIgnored(int index, string message)
        {
            var text = index.ToString();
            if (text.StartsWith("233")) return "";
            if (text.StartsWith("666")) return "";
            if (text.StartsWith("11451")) return "恶臭车牌爬";
            if (text.EndsWith("000")) return "";
            if (text.StartsWith("12345") || text.StartsWith("23456")) return "";
            var trimed = message.Trim().ToLower();

            if (trimed.IndexOf("大分") > 0) return null;
            if (trimed.IndexOf("自由") > 0) return null;
            if (trimed.IndexOf("q4") > 0) return null;
            if (trimed.IndexOf("q3") > 0) return null;
            if (trimed.IndexOf("q2") > 0) return null;
            if (trimed.IndexOf("q1") > 0) return null;
            if (trimed.IndexOf("m") > 0) return null;
            if (trimed.IndexOf("18w") > 0) return null;
            if (trimed.IndexOf("12w") > 0) return null;
            if (trimed.IndexOf("7w") > 0) return null;

            if (trimed.Length < 4)
            {
                return "描述信息过少将被视作无意义车牌，请增加描述如q2,18w等";
            }
            return null;
        }
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
                    var ignore = CheckIgnored(car, message.Substring(split));
                    if (ignore != null)
                    {
                        if (ignore != "")
                            await args.Callback(ignore);
                        break;
                    }
                    JObject res = await Utils.GetHttp($"http://api.bandoristation.com/?function=submit_room_number&number={car}&source={source}&token={token}&raw_message={raw_message}&user_id={args.Sender.FromQQ}");
                    if (res == null)
                    {
                        await args.Callback($"无法连接到bandoristation.com");
                    }
                    else if (res["status"].ToString() != "success")
                    {
                        await args.Callback($"上传车牌时发生错误: {res["response"]}");
                    }
                    return true;
                case CarType.Sekai:
                    if (CheckIgnored(car, message.Substring(split)) == "") break;
                    if (car == 114514 || car == 11451 || car == 19198) break;
                    var caro = new Car
                    {
                        index = car,
                        rawmessage = raw_message,
                        time = DateTime.UtcNow
                    };
                    lock (sekaicars)
                        sekaicars.Add(caro);
                    OnNewCar?.Invoke(caro);
                    return true;
            }

            return false;
        }
    }
}
