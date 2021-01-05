using BandoriBot;
using BandoriBot.Config;
using BandoriBot.Handler;
using Newtonsoft.Json.Linq;
using PCRClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PCRClientTest
{
    public class PCRManager
    {
        public static PCRManager Instance = new PCRManager();
        public PCRClient.PCRClient client;

        public void Do_Login()
        {
            client = new PCRClient.PCRClient(new EnvironmentInfo());
            client.Login("432399396", "5e09f022e619c1c9e0c34fc742ff98da_sh");
        }

        public PCRManager()
        {
            Do_Login();
        }

        public JObject GetRankInfo(int rank)
        {
            return client.Callapi("clan_battle/period_ranking", new JObject
            {
                ["clan_id"] = client.ClanId,
                ["clan_battle_id"] = client.ClanBattleid,
                ["period"] = 1,
                ["month"] = 0,
                ["page"] = (rank - 1) / 10,
                ["is_my_clan"] = 0,
                ["is_first"] = 0
            })["period_ranking"].Single(t => (int)t["rank"] == rank) as JObject;
        }

        public long GetRankDamage(int rank)
        {
            JObject obj;
            try
            {
                obj = GetRankInfo(rank);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
            catch (ApiException)
            {
                Do_Login();
                return 0;
            }

            return obj.Value<long>("damage");
        }

        public string GetRankStatistic(int rank)
        {
            JObject obj;
            try
            {
                obj = GetRankInfo(rank);
            }
            catch (InvalidOperationException)
            {
                return "找不到工会";
            }
            catch (Exception e)
            {
                Do_Login();
                return $"与服务器通讯时发生错误\"{e.Message}\", 正在重连中";
            }

            long score = obj.Value<long>("damage");
            string name = obj.Value<string>("clan_name");

            int lap = 1, order = 1;
            int t = 0;
            Boss o = null;
            while (true)
            {
                o = Configuration.GetConfig<PCRConfig>().bossInfo[order - 1];
                var value = (int)(o.value * o.multiplier[Math.Min(o.multiplier.Length, lap) - 1]);
                if (value + t > score) break;
                order++;
                if (order == 6)
                {
                    order = 1;
                    lap++;
                }
                t += value;
            }

            int remaining = (int)(o.value - (score - t) / o.multiplier[Math.Min(o.multiplier.Length, lap) - 1]);

            return $"在排名{rank}的公会是{name}，目前处于第{lap}周目，{o.name}，剩余血量{remaining}";
        }
    }
}
