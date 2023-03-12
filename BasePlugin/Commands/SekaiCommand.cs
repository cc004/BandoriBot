using BandoriBot.Config;
using BandoriBot.Models;
using Newtonsoft.Json;
using SekaiClient;
using SekaiClient.Datas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class SekaiGachaCommand : ICommand
    {
        public List<string> Alias => new List<string> { "pjsk抽卡" };

        public async Task Run(CommandArgs args)
        {
            if (args.Arg != "") return;
            var callback = args.Callback;
            var source = args.Source;
            args.finishedTask = Task.Run(() =>
            {
                try
                {
                    try
                    {
                        callback("抽卡进行中，不要着急哦");
                        var client = new SekaiClient.SekaiClient(new EnvironmentInfo())
                        {
                            DebugWrite = Console.WriteLine
                        };
                        client.InitializeAdid();
                        //client.UpgradeEnvironment().Wait();
                        var user = client.Register().Result;
                        client.Login(user).Wait();
                        var currency = client.PassTutorial().Result;
                        var result = string.Join("\n", client.Gacha(currency).Result);
                        var inherit = client.Inherit(Configuration.GetConfig<TokenConfig>().t.TryGetValue(source.FromQQ, out var val) ? val : source.FromQQ.ToString()).Result;
                        this.Log(LoggerLevel.Info, $"gacha inherit for user {source.FromQQ}: {inherit}");
                        callback($"[mirai:at={source.FromQQ}]抽卡结果：\n{result}\n引继码{inherit}，密码是默认你的qq哦（可以通过/token+密码进行设置）".ToImageText());
                    }
                    catch
                    {
                        callback("抽卡出错了哦，可能是服务器网络不好");
                    }
                }
                catch { }
            });
        }
    }

    public class SekaiLineCommand : ICommand
    {
        public List<string> Alias => new List<string> { "sekai线" };

        public async Task Run(CommandArgs args)
        {
            var track = await Utils.GetHttp($"https://bitbucket.org/sekai-world/sekai-event-track/raw/main/event{MasterData.Instance.CurrentEvent.id}.json");
            await args.Callback(string.Join('\n', new int[] { 100, 500, 1000, 2000, 5000, 10000, 50000 }.Select(i => $"rank{i} pt={track[$"rank{i}"].Single()["score"]}")));
        }
    }

    public partial class SekaiCommand : ICommand
    {

        public List<string> Alias => new List<string> { "sekai" , "sekaitext"};
        private PPHManager manager;
        private static int eventId => MasterData.Instance.CurrentEvent.id;
        
        public SekaiCommand()
        {
            manager = new PPHManager(this);
            manager.Initialize();
        }

        private Dictionary<int, int> scoreCache;
        private DateTime lastref;
        private bool refreshing = false;

        private void RefreshCache()
        {
            if (refreshing) return;
            try
            {
                refreshing = true;
                var now = DateTime.Now;
                if (now - lastref < new TimeSpan(0,  30, 0)) return;
                scoreCache = (Utils.GetHttp("https://pjsk.oss-cn-hangzhou.aliyuncs.com/predict.json").Result)["data"].ToObject<Dictionary<string, string>>()
                    .Where(pair => int.TryParse(pair.Key, out var _)).ToDictionary(pair => int.Parse(pair.Key), pair => int.Parse(pair.Value));
                lastref = now;
            }
            finally
            {
                refreshing = false;
            }
        }

        public static async Task<string> Query(long rankorid)
        {
            var client = SekaiClient.SekaiClient.GetClient();
            var result = (await (rankorid > int.MaxValue ?
                client.CallUserApi($"/event/{eventId}/ranking?targetUserId={rankorid}", HttpMethod.Get, null) :
                client.CallUserApi($"/event/{eventId}/ranking?targetRank={rankorid}", HttpMethod.Get, null))).ToString(Formatting.Indented);
            SekaiClient.SekaiClient.PutClient(client);
            return result;
        }

        private async Task<string> GetDesc(int rankacc, SekaiClient.SekaiClient client)
        {
            if (rankacc == 0) return string.Empty;
            var score = (await client.CallUserApi($"/event/{eventId}/ranking?targetRank={rankacc}", HttpMethod.Get, null))["rankings"][0]["score"];

            return $"排名{rankacc}当前分数{score}" +
                   $"{(manager.hourSpeed.TryGetValue(rankacc, out var val) ? $"({val}pt/h)" : string.Empty)}，" +
                   $"预测{(scoreCache == null ? -1 : scoreCache.TryGetValue(rankacc, out var v) ? v : 0)}";
        }

        private static readonly int[] ranks = new int[]
        {
            100, 200, 500, 1000, 2000, 5000, 10000, 20000, 50000, 100000
        };

        private async Task<string> GetPred(int rank, SekaiClient.SekaiClient client)
        {
            try
            {
                var sb = new StringBuilder();
                var up = ranks.LastOrDefault(r => r < rank);
                if (up != 0)
                    sb.Append($"\n上一档{await GetDesc(up, client)}");
                var down = ranks.FirstOrDefault(r => r >= rank);
                if (down != 0)
                    sb.Append($"\n下一档{await GetDesc(down, client)}");
                return sb.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task Run(CommandArgs args)
        {
            long arg;
            int eventId = SekaiCommand.eventId;

            if (string.IsNullOrEmpty(args.Arg))
            {
                try
                {
                    arg = Configuration.GetConfig<SekaiCache>().t[args.Source.FromQQ];
                }
                catch
                {
                    await args.Callback("你还没有绑定你的id哦");
                    return;
                }
            }
            else
            {
                var split = args.Arg.Trim().Split(':');
                if (!long.TryParse(split[0], out arg)) return;
                if (split.Length == 2 && !int.TryParse(split[1], out eventId)) eventId = SekaiCommand.eventId;

                Configuration.GetConfig<SekaiCache>().t[args.Source.FromQQ] = arg;
                Configuration.GetConfig<SekaiCache>().Save();
            }


            args.finishedTask = Task.Run(async () =>
            {
                try
                {
                    var client = SekaiClient.SekaiClient.GetClient();
                    var result = await (arg > int.MaxValue ?
                        client.CallUserApi($"/event/{eventId}/ranking?targetUserId={arg}", HttpMethod.Get, null) :
                        client.CallUserApi($"/event/{eventId}/ranking?targetRank={arg}", HttpMethod.Get, null));
                    var rank = result["rankings"]?.SingleOrDefault();

                    var text = rank == null
                        ? "找不到玩家"
                        : ($"排名为{rank["rank"]}的玩家是`{rank["name"]}`(uid={rank["userId"]})，分数为{rank["score"]}" +
                           await GetPred((int)rank["rank"], client));
                    SekaiClient.SekaiClient.PutClient(client);
                    try
                    {
                        this.Log(LoggerLevel.Debug, text);
                        if (args.Trigger == "sekai")
                            await args.Callback(text.ToImageText());
                        else
                            await args.Callback(text);
                        RefreshCache();
                    }
                    catch (Exception e)
                    {
                        this.Log(LoggerLevel.Error, e.ToString());
                    }
                }
                catch
                {
                }
            });
        }
    }
}
