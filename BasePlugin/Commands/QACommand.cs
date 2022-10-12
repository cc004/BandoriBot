using System;
using BandoriBot.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AssetsTools;
using BandoriBot.Services;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PCRApi;
using PCRApi.Controllers;
using PCRApi.Models.Db;

namespace BandoriBot.Commands
{
    public class QACommand : ICommand
    {
        public List<string> Alias => new List<string> { "/qa", "QA版本更新 Manifest:", "QA版本更新 manifest:" };

        private string manifest;

        public async Task Run(CommandArgs args)
        {
            var mgr = AssetController.manager;
            var a = args.Arg.Trim().Split(' ');
            if (a[0] == "json")
            {
                await args.Callback(manifest);
                return;
            }
            if (long.TryParse(a[0], out var val)) a = new[] {"update_query", a[0]};
            if (a[0].IndexOf("update") >= 0)
            {
                var client = new AssetController.PCRClient();
                client.urlroot = "http://l3-qa2-all-gs-gzlj.bilibiligame.net/";
                var manifest = client.Callapi("source_ini/get_resource_info", new JObject { ["viewer_id"] = "0" });
                await masterContextCache.instance.Database.CloseConnectionAsync();
                await masterContextCache.instance.DisposeAsync();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                try
                {
                    await mgr.Initialize(a[1],
                        (string) manifest["movie_ver"],
                        (string) manifest["sound_ver"], manifest["resource"][0].ToString(), true);
                }
                finally
                {
                    masterContextCache.instance = new masterContext();
                    MasterDataContext.Instance = new();
                }
                await args.Callback($"manifest updated to {a[1]}\n");
                this.manifest = new JObject
                {
                    ["data"] = new JObject
                    {
                        ["movie"] = manifest["movie_ver"],
                        ["sound"] = manifest["sound_ver"],
                        ["patch"] = "",
                        ["manifest"] = a[1]
                    }
                }.ToString();
            }
            if (a[0].IndexOf("query") >= 0)
            {
                var now = DateTime.Now;
                var res = "公主连结半月刊:\n" + string.Join("\n", ISchedule.Schedules.SelectMany(sch =>
                    {
                        try
                        {
                            return sch.AsEnumerable().ToArray();
                        }
                        catch (Exception e) 
                        {
                            return Array.Empty<ISchedule>();
                        }
                    })
                    .Where(s => DateTime.Parse(s.StartTime) > now && s.Enabled)
                    .Select(s => ((
                        $"{DateTime.Parse(s.StartTime).ToShortDateString()}-{DateTime.Parse(s.EndTime).ToShortDateString()}",
                        DateTime.Parse(s.StartTime), s.GetDescription())))
                    .GroupBy(t => t.Item1)
                    .OrderBy(g => g.First().Item2)
                    .Select(g => $"{g.Key}\n{string.Join("\n", g.Select(s => $"      {s.Item3}"))}"));
                if (a[0].IndexOf("text") >= 0)
                    await args.Callback(res);
                else
                    await args.Callback(res.ToImageText());
            }
        }
    }
}
