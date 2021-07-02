using BandoriBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SekaiClient;
using SekaiClient.Datas;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
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
            new System.Threading.Thread(() =>
            {
                try
                {
                    try
                    {
                        callback("抽卡进行中，不要着急哦");
                        var client = SekaiClient.SekaiClient.StaticClient;
                        client.InitializeAdid();
                        client.UpgradeEnvironment().Wait();
                        var user = client.Register().Result;
                        client.Login(user).Wait();
                        var currency = client.PassTutorial().Result;
                        var result = string.Join("\n", client.Gacha(currency).Result);
                        var inherit = client.Inherit(source.FromQQ.ToString()).Result;
                        callback($"[mirai:at={source.FromQQ}]抽卡结果：\n{result}\n引继码已经私聊你了，密码是你的qq哦（加好友才可以私聊）");
                        try
                        {
                            this.Log(LoggerLevel.Info, $"gacha inherit for user {source.FromQQ}: {inherit}");
                            source.Session.SendFriendMessageAsync(source.FromQQ, new Mirai_CSharp.Models.PlainMessage($"引继id: {inherit}"));
                        }
                        catch { }
                    }
                    catch
                    {
                        callback("抽卡出错了哦，可能是服务器网络不好");
                    }
                }
                catch { }
            }).Start();
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

    public class SekaiPCommand : ICommand
    {
        public List<string> Alias => new List<string> { "sekai预测" };

        private static Tuple<double, double> LSE(double[] x, double[] y)
        {
            var n = x.Length;
            double avgx = x.Sum() / n, avgy = y.Sum() / n;

            double k = Enumerable.Range(0, n).Sum(i => (x[i] - avgx) * (y[i] - avgy)) / Enumerable.Range(0, n).Sum(i => (x[i] - avgx) * (x[i] - avgx));
            double b = avgy - k * avgx;

            return new Tuple<double, double>(k, b);
        }

        private static double PassZero(double[] x, double[] y)
        {
            var n = x.Length;
            return Enumerable.Range(0, n).Sum(i => x[i] * y[i]) / Enumerable.Range(0, n).Sum(i => x[i] * x[i]);
        }

        public async Task Run(CommandArgs args)
        {
            await args.Callback("本功能已弃用，请使用predsekai获取分数线");
            return;
            var evt = MasterData.Instance.CurrentEvent;

            RankData data;
            
            lock (Program.SekaiFile)
                data = RankData.FromFile($"sekai_event{evt.id}.csv");


            int.TryParse(args.Arg, out int rank);

            if (!data.ranks.ContainsKey(rank))
            {
                await args.Callback("排名数据不存在");
                return;
            }

            var x = data.timestamp.Select(l => (double)l).ToArray();
            var y = data.ranks[rank].Select(l => (double)l).ToArray();

            var fit = LSE(x, y);
            var fit2 = PassZero(x.Select(l => l - evt.startAt).ToArray(), y);

            await args.Callback($"排名{rank}的预测分数为\n{(int)(fit.Item1 * evt.aggregateAt + fit.Item2)} (LSE)\n{(int)(fit2 * (evt.aggregateAt - evt.startAt))} (过原点)");
        }
    }


    public partial class SekaiCommand : ICommand
    {

        public List<string> Alias => new List<string> { "sekai" };
        private SekaiClient.SekaiClient client;
        private SekaiClient.SekaiClient clientForManager;
        private PPHManager manager;
        private int eventId;

        private async Task<SekaiClient.SekaiClient> CacheGetClient(string filename)
        {
            SekaiClient.SekaiClient client;
            while (true)
            {
                try
                {
                    client = new SekaiClient.SekaiClient(new EnvironmentInfo(), false)
                    {
                        DebugWrite = text =>
                        {
                            var stack = new StackTrace();
                            var method = stack.GetFrame(1).GetMethod();
                            this.Log(LoggerLevel.Debug, $"[{method.DeclaringType.Name}::{method.Name}]".PadRight(32) + text);
                        }
                    };

                    await client.UpgradeEnvironment();
                    User user;
                    try
                    {
                        user = JsonConvert.DeserializeObject<User>(File.ReadAllText(filename));
                        await client.Login(user);
                        await MasterData.Initialize(client);
                    }
                    catch
                    {
                        user = await client.Register();
                        await client.Login(user);
                        await MasterData.Initialize(client);
                        await client.PassTutorial(true);
                    }
                    File.WriteAllText(filename, JsonConvert.SerializeObject(user));
                    break;
                }
                catch (Exception e)
                {
                    this.Log(LoggerLevel.Error, e.ToString());
                    await Task.Delay(10000);
                }
            }
            return client;
        }

        private async Task ClientReady()
        {
            client = await CacheGetClient("sekaiuser.json");
            eventId = MasterData.Instance.CurrentEvent.id;
        }
        private async Task ManagerClientReady()
        {
            clientForManager = await CacheGetClient("sekaiuser2.json");
        }

        public SekaiCommand()
        {
            if (File.Exists("sekai"))
            {
                ClientReady().Wait();
                ManagerClientReady().Wait();
                manager = new PPHManager(this);
                manager.Initialize();
            }
        }

        private Dictionary<int, int> scoreCache;
        private DateTime lastref;
        private async Task RefreshCache()
        {
            var now = DateTime.Now;
            if (now - lastref < new TimeSpan(0, 1, 0)) return;
            scoreCache = (await Utils.GetHttp("https://api.sekai.best/event/pred"))["data"].ToObject<Dictionary<string, long>>()
                .Where(pair => int.TryParse(pair.Key, out var _)).ToDictionary(pair => int.Parse(pair.Key), pair => (int)pair.Value);
            lastref = now;
        }

        private async Task<string> GetDesc(int rankacc)
        {
            if (rankacc == 0) return string.Empty;
            var score = (await client.CallUserApi($"/event/{eventId}/ranking?targetRank={rankacc}", HttpMethod.Get, null))["rankings"][0]["score"];

            return $"排名{rankacc}当前分数{score}" +
                $"{(manager.hourSpeed.TryGetValue(rankacc, out var val) ? $"({val}pt/h)" : string.Empty)}，" +
                $"预测{scoreCache[rankacc]}";
        }

        private static readonly int[] ranks = new int[]
        {
            100, 500, 1000, 5000, 10000, 50000, 100000
        };

        private async Task<string> GetPred(int rank)
        {
            try
            {
                var sb = new StringBuilder();
                var up = ranks.LastOrDefault(r => r < rank);
                if (up != 0)
                    sb.Append($"\n上一档{await GetDesc(up)}");
                var down = ranks.FirstOrDefault(r => r >= rank);
                if (down != 0)
                    sb.Append($"\n下一档{await GetDesc(down)}");
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
            try
            {
                arg = long.Parse(args.Arg.Trim());
            }
            catch
            {
                return;
            }

            try
            {
                var result = await (arg > int.MaxValue ?
                    client.CallUserApi($"/event/{eventId}/ranking?targetUserId={arg}", HttpMethod.Get, null) :
                    client.CallUserApi($"/event/{eventId}/ranking?targetRank={arg}", HttpMethod.Get, null));
                var rank = result["rankings"]?.SingleOrDefault();

                await args.Callback(rank == null ? "找不到玩家" : $"排名为{rank["rank"]}的玩家是`{rank["name"]}`(uid={rank["userId"]})，分数为{rank["score"]}" + await GetPred((int)rank["rank"]));
                await RefreshCache();
            }
            catch (Exception e)
            {
                this.Log(LoggerLevel.Debug, e.ToString());
                await ClientReady();
            }
        }
    }
}
