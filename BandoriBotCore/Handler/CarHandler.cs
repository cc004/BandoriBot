using BandoriBot.Config;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Web;

namespace BandoriBot.Handler
{
    public class CarHandler : IMessageHandler
    {
        private static string source = HttpUtility.UrlEncode("冲冲");
        private static string token = "2NmWeiklE";

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

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            int split, car;
            if (IsIgnore(Sender)) return false;
            if (message.Length < 5) return false;
            for (int i = 0; i < 5; ++i)
                if (!IsNumeric(message[i])) return false;
            split = message.Length > 5 && IsNumeric(message[5]) ? 6 : 5;
            if (message.Length > split && IsNumeric(message[split])) return false;

            car = int.Parse(message.Substring(0, split));
            if (car == 114514)
            {
                callback("恶意车牌已自动屏蔽");
                return true;
            }

            if (message.IndexOf("彩黑") != -1)
            {
                message = message.Replace("彩黑", "彩 黑");
            }

            Thread.Sleep(Configuration.GetConfig<Delay>()[Sender.FromQQ] * 1000);

            string raw_message = car.ToString() + " " + message.Substring(split);
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
        }
    }
}
