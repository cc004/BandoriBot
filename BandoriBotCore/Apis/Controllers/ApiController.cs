using System;
using BandoriBot.Handler;
using BandoriBot.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BandoriBot.Config;
using BandoriBot.Models;
using Microsoft.EntityFrameworkCore;
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

        private static double calcHash(string str)
        {
            var text = Encoding.ASCII.GetBytes(str);
            uint _0x473e93, _0x5d587e;
            for (_0x473e93 = 0x1bf52, _0x5d587e = (uint)text.Length; _0x5d587e != 0;)
                _0x473e93 = 0x309 * _0x473e93 ^ text[--_0x5d587e];
            return _0x473e93 >> 0x3;
        }

        [HttpPost("pcrd")]
        public async Task<string> Pcrd(Request request)
        {
            try
            {
                if (request.def.Length == 0)
                    return new JObject()
                    {
                        ["code"] = 400,
                        ["message"] = "team has no member"
                    }.ToString(Formatting.Indented);

                var nonce = GenNonce();
                var json = new JObject
                {
                    ["def"] = new JArray(request.def.Distinct()),
                    ["language"] = 0,
                    ["nonce"] = nonce,
                    ["page"] = request.page,
                    ["region"] = request.region,
                    ["sort"] = request.sort,
                    ["ts"] = GetTimeStamp()
                };

                await JJCManager.Instance.UpdateVersion();
                var sign = JJCManager.Instance.wrapper.RunEvent(1, new object[]
                {
                    json.ToString(Formatting.None),
                    nonce,
                    calcHash(nonce)
                }) as string;

                json = new JObject
                {
                    ["_sign"] = sign,
                    ["def"] = json["def"],
                    ["language"] = json["language"],
                    ["nonce"] = json["nonce"],
                    ["page"] = json["page"],
                    ["region"] = json["region"],
                    ["sort"] = json["sort"],
                    ["ts"] = json["ts"]
                };
                
                var result = client.PostAsync($"https://api.pcrdfans.com/x/v1/search",
                    new StringContent(json.ToString(Formatting.None)
                        , Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync().Result;

                if (!result.Contains("\"data\""))
                {
                    this.Log(LoggerLevel.Error, $"pcrd ret err: {result} while processing request: {json}");
                }
                return result;
            }
            catch (Exception ex)
            {
                return new JObject()
                {
                    ["code"] = 500,
                    ["message"] = ex.ToString()
                }.ToString(Formatting.Indented);
            }

        }
        [HttpGet("execute")]
        public async Task<ActionResult<string>> Execute(string message)
        {
            if (!await CheckPermission("execute")) return BadRequest();
            var source = new Source { FromGroup = 0, FromQQ = GetUID(), Session = MessageHandler.session };
            var result = new StringBuilder();
            var args = new HandlerArgs
            {
                Sender = source,
                Callback = async s => result.AppendLine(s),
                message = message
            };
            await MessageHandler.OnMessage(args);
            await args.finishedTask;
            return result.ToString();
        }
        /*
        [HttpGet("sekai")]
        public async Task<ActionResult<string>> Sekai(long id)
        {
            return await SekaiCommand.Query(id);
        }
        */
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
            IQueryable<Record> result = RecordDatabaseManager.GetRecords();
            if (qq > 0) result = result.Where(r => r.qq == qq);
            if (group > 0) result = result.Where(r => r.group == group);
            if (starttime > 0) result = result.Where(r => r.timestamp >= starttime);
            if (endtime > 0) result = result.Where(r => r.timestamp <= endtime);
            if (!string.IsNullOrEmpty(keyword)) result = result.Where(r => r.message.Contains(keyword));
            return result.Take(limit).ToArray();
        }
        
        [HttpGet("countv2")]
        public async Task<ActionResult<string>> Countv2(string keyword = null, long qq = 0, long group = 0, long starttime = 0, long endtime = 0)
        {
            if (!await CheckPermission("get")) return BadRequest();
            IQueryable<Record> result = RecordDatabaseManager.GetRecords();
            if (qq > 0) result = result.Where(r => r.qq == qq);
            if (group > 0) result = result.Where(r => r.group == group);
            if (starttime > 0) result = result.Where(r => r.timestamp >= starttime);
            if (endtime > 0) result = result.Where(r => r.timestamp <= endtime);
            if (!string.IsNullOrEmpty(keyword)) result = result.Where(r => r.message.Contains(keyword));
            return result.Count().ToString();
        }
    }
}
