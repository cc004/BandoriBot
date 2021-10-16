using System;
using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BandoriBot.Apis.Controllers
{
    public class TimelineData
    {
        public string Data { get; set; }
        public string Name { get; set; }
    }

    public class Request
    {
        public int[] def { get; set; }
        public int page { get; set; }
        public int region { get; set; }
        public int sort { get; set; }
    }
    
    [ApiController]
    public class ApiController : Controller
    {
        private long GetUID() => long.Parse(Request.Query["uid"]);
        private async Task<bool> CheckPermission(string perm)
        {
            long uid = GetUID();
            var token = Request.Query["token"];
            return Configuration.GetConfig<TokenConfig>().t.TryGetValue(uid, out var t) && t == token &&
                await new Source { FromQQ = uid }.HasPermission("rest." + perm, -1);
        }

        private static HttpClient client = new HttpClient();

        static ApiController()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 Edg/87.0.664.66");
            client.DefaultRequestHeaders.Add("Referer", "https://pcrdfans.com/");
            client.DefaultRequestHeaders.Add("Origin", "https://pcrdfans.com");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            client.DefaultRequestHeaders.Remove("Expect");
            client.Timeout = new TimeSpan(0, 0, 10);
        }

        private static string GenNonce()
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            var rand = new Random();

            return new string(Enumerable.Range(0, 16).Select(_ => chars[rand.Next(36)]).ToArray());
        }
        private static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (long)ts.TotalSeconds;
        }

        [HttpPost("pcrd")]
        public async Task<string> Pcrd(Request request)
        {
            var json = new JObject
            {
                ["def"] = new JArray(request.def.Distinct()),
                ["nonce"] = GenNonce(),
                ["page"] = request.page,
                ["region"] = request.region,
                ["sort"] = request.sort,
                ["ts"] = GetTimeStamp()
            };
            
            string sign = null;

            JJCManager.GetSign(json.ToString(Formatting.None), json.Value<string>("nonce"),
                s => sign = s);


            json = new JObject
            {
                ["_sign"] = sign,
                ["def"] = json["def"],
                ["nonce"] = json["nonce"],
                ["page"] = json["page"],
                ["region"] = json["region"],
                ["sort"] = json["sort"],
                ["ts"] = json["ts"]
            };
            
            JObject raw = null;

            return client.PostAsync($"https://api.pcrdfans.com/x/v1/search",
                new StringContent(json.ToString(Formatting.None)
                    , Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync().Result;

        }
        [HttpGet("execute")]
        public async Task<ActionResult<string>> Execute(string message)
        {
            var source = new Source { FromGroup = 0, IsTemp = false, FromQQ = GetUID(), Session = MessageHandler.session };
            var result = new StringBuilder();
            await MessageHandler.instance.OnMessage(new HandlerArgs
            {
                Sender = source,
                Callback = async s => result.AppendLine(s),
                message = message
            });
            return result.ToString();
        }

        [HttpGet("count")]
        public async Task<ActionResult<string>> Count(string keyword)
        {
            if (!await CheckPermission("count")) return BadRequest();
            return RecordDatabaseManager.CountContains(keyword).ToString();
        }

        [HttpGet("get")]
        public async Task<ActionResult<Record[]>> Get(string keyword = null, long qq = 0, long group = 0, long starttime = 0, long endtime = 0, int limit = 100)
        {
            if (!await CheckPermission("get")) return BadRequest();
            var result = RecordDatabaseManager.GetRecords();
            if (qq > 0) result = result.Where(r => r.qq == qq);
            if (group > 0) result = result.Where(r => r.group == group);
            if (starttime > 0) result = result.Where(r => r.timestamp >= starttime);
            if (endtime > 0) result = result.Where(r => r.timestamp <= endtime);
            if (!string.IsNullOrEmpty(keyword)) result = result.Where(r => r.message.Contains(keyword));
            return result.Take(limit).ToArray();
        }

        private static readonly object iolock = new object();

        private static readonly Regex reg = new Regex(@"-4010([234])(\d\d)0([1-5])）造成(\d*)伤害", RegexOptions.Compiled);

        private static string Fix(string name)
        {
            var s = name.Split('-', '.');
            return $"{s[1]}-{s[2]}-{s[3]}-{s[0]}.txt";
        }
        [HttpGet("history")]
        public async Task<ActionResult<string>> PostRecord(long group)
        {
            return Configuration.GetConfig<Pipe>().GetHistory(group);
        }
        [HttpGet("countv2")]
        public async Task<ActionResult<string>> Countv2(string keyword = null, long qq = 0, long group = 0, long starttime = 0, long endtime = 0)
        {
            if (!await CheckPermission("get")) return BadRequest();
            var result = RecordDatabaseManager.GetRecords();
            if (qq > 0) result = result.Where(r => r.qq == qq);
            if (group > 0) result = result.Where(r => r.group == group);
            if (starttime > 0) result = result.Where(r => r.timestamp >= starttime);
            if (endtime > 0) result = result.Where(r => r.timestamp <= endtime);
            if (!string.IsNullOrEmpty(keyword)) result = result.Where(r => r.message.Contains(keyword));
            return result.Count().ToString();
        }
    }
}
