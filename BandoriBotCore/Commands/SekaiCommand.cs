using BandoriBot.Models;
using Newtonsoft.Json.Linq;
using SekaiClient.Datas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class SekaiLineCommand : ICommand
    {
        public List<string> Alias => new List<string> { "sekai线" };

        public void Run(CommandArgs args)
        {
            var track = Utils.GetHttp($"https://bitbucket.org/sekai-world/sekai-event-track/raw/main/event{MasterData.Instance.events.Last().id}.json");
            args.Callback(string.Join('\n', new int[] { 100, 500, 1000, 2000, 5000, 10000, 50000 }.Select(i => $"rank{i} pt={track[$"rank{i}"].Single()["score"]}")));
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

        public void Run(CommandArgs args)
        {
            var evt = MasterData.Instance.events.Last();

            RankData data;
            
            lock (Program.SekaiFile)
                data = RankData.FromFile($"sekai_event{evt.id}.csv");


            int.TryParse(args.Arg, out int rank);

            if (!data.ranks.ContainsKey(rank))
            {
                args.Callback("排名数据不存在");
                return;
            }

            var x = data.timestamp.Select(l => (double)l).ToArray();
            var y = data.ranks[rank].Select(l => (double)l).ToArray();

            var fit = LSE(x, y);
            var fit2 = PassZero(x.Select(l => l - evt.startAt).ToArray(), y);

            args.Callback($"排名{rank}的预测分数为\n{(int)(fit.Item1 * evt.aggregateAt + fit.Item2)} (LSE)\n{(int)(fit2 * (evt.aggregateAt - evt.startAt))} (过原点)");
        }
    }


    public class SekaiCommand : ICommand
    {
        public List<string> Alias => new List<string> { "sekai" };
        private SekaiClient.SekaiClient client;
        private int eventId;

        private async Task ClientReady()
        {
            while (true)
            {
                try
                {
                    client = new SekaiClient.SekaiClient(new SekaiClient.EnvironmentInfo(), false);
                    await client.UpgradeEnvironment();
                    await client.Login(await client.Register());
                    await MasterData.Initialize(client);
                    await client.PassTutorial(true);
                    break;
                }
                catch (Exception e)
                {
                    this.Log(LoggerLevel.Error, e.ToString());
                }
            }
            eventId = MasterData.Instance.events.Last().id;
        }

        public SekaiCommand()
        {
            if (File.Exists("sekai"))
                ClientReady().Wait();
        }

        public void Run(CommandArgs args)
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
                var result = (arg > int.MaxValue ?
                    client.CallUserApi($"/event/{eventId}/ranking?targetUserId={arg}", HttpMethod.Get, null) :
                    client.CallUserApi($"/event/{eventId}/ranking?targetRank={arg}", HttpMethod.Get, null)).Result;
                var rank = result["rankings"]?.SingleOrDefault();

                args.Callback(rank == null ? "找不到玩家" : $"排名为{rank["rank"]}的玩家是`{rank["name"]}`(uid={rank["userId"]})，分数为{rank["score"]}");
            }
            catch (Exception e)
            {
                this.Log(LoggerLevel.Debug, e.ToString());
                ClientReady().Wait();
            }
        }
    }
}
