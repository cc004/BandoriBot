using System;
using BandoriBot.Config;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AssetsTools;
using BandoriBot.Services;
using Newtonsoft.Json.Linq;
using PCRApi;
using PCRApi.Controllers;
using PCRApi.Models.Db;

namespace BandoriBot.Commands
{
    public class QACommand : ICommand
    {
        private AssetManager mgr = new ();

        public List<string> Alias => new List<string> { "/qa", "QA版本更新 Manifest:" };


        public async Task Run(CommandArgs args)
        {
            var a = args.Arg.Trim().Split(' ');
            if (long.TryParse(a[0], out var val)) a = new[] {"update_query", a[0]};
            if (a[0].IndexOf("update") >= 0)
            {
                var client = new AssetController.PCRClient();
                client.urlroot = "http://l3-qa2-all-gs-gzlj.bilibiligame.net/";
                var manifest = client.Callapi("source_ini/get_resource_info", new JObject { ["viewer_id"] = "0" });

                await mgr.Initialize(a[1],
                    (string)manifest["movie_ver"],
                    (string)manifest["sound_ver"], manifest["resource"][0].ToString());
                var ab = await mgr.ResolveAssetsBundle("a/masterdata_master.unity3d", "master_data.unity3d");
                var af = ab.Files[0].ToAssetsFile();
                await File.WriteAllBytesAsync("Data/master.db", af.Objects[0].Data.Skip(16).ToArray());
                masterContextCache.instance = new masterContext();
                await args.Callback($"manifest updated to {a[1]}");
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
                        DateTime.Parse(s.StartTime), s.Description)))
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
