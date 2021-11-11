using BandoriBot.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{


    public partial class SekaiCommand
    {
        private class PPHManager
        {
            private Dictionary<int, int> hourCache = new Dictionary<int, int>();
            public Dictionary<int, int> hourSpeed = new Dictionary<int, int>();
            private SekaiCommand parent;

            public PPHManager(SekaiCommand parent)
            {
                this.parent = parent;
            }

            private async Task RefreshCache()
            {
                try
                {
                    foreach (var rank in ranks)
                    {
                        var resp = await SekaiClient.SekaiClient.StaticClient.CallUserApi($"/event/{eventId}/ranking?targetRank={rank}", HttpMethod.Get, null);
                        var now = resp["rankings"][0].Value<int>("score");

                        if (hourCache.ContainsKey(rank)) hourSpeed[rank] = now - hourCache[rank];
                        hourCache[rank] = now;
                    }
                }
                catch (Exception e)
                {
                    this.Log(LoggerLevel.Warn, e.ToString());
                    await SekaiClient.SekaiClient.StaticClient.Reset();
                }
            }
            public void Initialize()
            {
                new Thread(() =>
                {
                    while (true)
                    {
                        try
                        {
                            RefreshCache().Wait();
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            continue;
                        }
                        Thread.Sleep(1000 * 3600);
                    }
                }).Start();
            }
        }
    }
}
