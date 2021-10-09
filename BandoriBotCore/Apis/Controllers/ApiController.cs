using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot.Apis.Controllers
{
    public class TimelineData
    {
        public string Data { get; set; }
        public string Name { get; set; }
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
