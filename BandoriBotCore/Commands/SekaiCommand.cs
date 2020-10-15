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
                    await client.PassTutorial(true);
                    break;
                }
                catch (Exception e)
                {
                    this.Log(LoggerLevel.Error, e.ToString());
                }
            }
            //var master = await client.CallApi("/suite/master", HttpMethod.Get, null);
            //eventId = master["events"].Last().Value<int>("id");
            eventId = 1;
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
                var rank = result["rankings"].SingleOrDefault();

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
