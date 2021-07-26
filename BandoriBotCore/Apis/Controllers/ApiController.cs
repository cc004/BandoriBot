using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Apis.Controllers
{
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
            if (!await CheckPermission("execute")) return BadRequest();
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
        public async Task<ActionResult<Record[]>> Get(string keyword =null, long qq=0, long group=0, long starttime=0, long endtime=0)
        {
            if (!await CheckPermission("get")) return BadRequest();
            var result = RecordDatabaseManager.GetRecords();
            if (qq > 0) result = result.Where(r => r.qq == qq);
            if (group > 0) result = result.Where(r => r.group == group);
            if (starttime > 0) result = result.Where(r => r.timestamp >= starttime);
            if (endtime > 0) result = result.Where(r => r.timestamp <= endtime);
            if (!string.IsNullOrEmpty(keyword)) result = result.Where(r => r.message.Contains(keyword));
            return result.ToArray();
        }
    }
}
