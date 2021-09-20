using LazyUtils;
using LinqToDB;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rests;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using Wolfje.Plugins.SEconomy;

namespace RestCharacter
{
    class DeathTimes : PlayerConfigBase<DeathTimes>
    {
        public int times = 0;
    }
    class DailyOnlineTime : PlayerConfigBase<DailyOnlineTime>
    {
        public int time = 0;
    }
    class OnlineTime : PlayerConfigBase<OnlineTime>
    {
        public int time = 0;
    }

    public class InWhitelist : PlayerConfigBase<InWhitelist>
    {
        public bool status;
    }

    [ApiVersion(2, 1)]
    public class Class1 : TerrariaPlugin
    {
        public Class1(Main game) : base(game)
        {
        }

        public override string Name => "RestCharacter";
        public override void Initialize()
        {
            new Task(() =>
            {
                while(true)
                {
                    if (DateTime.Now.Hour == 5&&DateTime.Now.Minute ==1)
                    {
                        int i = 0;
                        using (var context = Db.Context<DailyOnlineTime>())
                            foreach (var o in context.Config.OrderByDescending(tuple => tuple.time))
                            {
                                i++;
                                UserAccount account = TShock.UserAccounts.GetUserAccountByName(o.name);
                                if (i <= 3)//如果排名前三则升级
                                {
                                    switch (account .Group)
                                    {
                                        case "default":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv1会员");
                                            }
                                            break;
                                        case "Lv1会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv2会员");
                                            }
                                            break;
                                        case "Lv2会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv3会员");
                                            }
                                            break;
                                        case "Lv3会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv4会员");
                                            }
                                            break;
                                        case "Lv4会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv5会员");
                                            }
                                            break;
                                        case "Lv5会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv6会员");
                                            }
                                            break;
                                    }
                                }
                                else//如果排名不为前三则降级
                                {
                                    switch (account.Group)
                                    {
                                        case "Lv1会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "default");
                                            }
                                            break;
                                        case "Lv2会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv1会员");
                                            }
                                            break;
                                        case "Lv3会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv2会员");
                                            }
                                            break;
                                        case "Lv4会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv3会员");
                                            }
                                            break;
                                        case "Lv5会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv4会员");
                                            }
                                            break;
                                        case "Lv6会员":
                                            {
                                                TShock.UserAccounts.SetUserGroup(account, "Lv5会员");
                                            }
                                            break;
                                    }
                                }
                            }
                        TShock.DB.QueryReader("update dailyonlinetime set time=0");
                    }
                    else
                    {
                        Thread.Sleep(1000 * 60);
                    }
                }
            }).Start();
            TShock.RestApi.Register(new SecureRestCommand("/v1/questrank/rankboard", (RestRequestArgs args) =>
            {
                var i = 0;
                return new JArray
                (
                    CompatHelper.GetUsers()
                        .Select((user) => new Tuple<string, PlayerData>(user.Name, TShock.CharacterDB.GetPlayerData(null, user.ID)))
                        .Where((tuple) => tuple.Item2.exists)
                        .Select(tuple => new Tuple<string, int>(tuple.Item1, tuple.Item2.questsCompleted))
                        .OrderByDescending((tuple) => tuple.Item2).Select((tuple) => new JObject
                        {
                            ["times"] = tuple.Item2,
                            ["rank"] = ++i,
                            ["name"] = tuple.Item1
                        }).ToArray()
                );
            }, new string[] { "questrank.rest" }));

            TShock.RestApi.Register(new SecureRestCommand("/v1/itemrank/rankboard", (RestRequestArgs args) =>
            {
                var id = int.Parse(args.Parameters["id"]);
                var i = 0;
                return new JArray
                (
                    CompatHelper.GetUsers()
                        .Select((user) => new Tuple<string, PlayerData>(user.Name, TShock.CharacterDB.GetPlayerData(null, user.ID)))
                        .Where((tuple) => tuple.Item2.exists)
                        .Select((tuple) => new Tuple<string, int>(tuple.Item1, tuple.Item2.inventory
                            .Where((item) => item.NetId == id)
                            .Sum((item) => item.Stack)))
                        .OrderByDescending((tuple) => tuple.Item2).Select((tuple) => new JObject
                        {
                            ["count"] = tuple.Item2,
                            ["rank"] = ++i,
                            ["name"] = tuple.Item1
                        }).ToArray()
                );
            }, new string[] { "itemrank.rest" }));
            int time = 0;

            if (GetDataHandlers.KillMe == null) GetDataHandlers.KillMe = new HandlerList<GetDataHandlers.KillMeEventArgs>();
            GetDataHandlers.KillMe.Register((_, args) =>
            {
                using (var context = Db.PlayerContext<DeathTimes>())
                    context.Get(TShock.Players[args.PlayerId]).Set(d => d.times, d => d.times + 1).Update();
            });

            ServerApi.Hooks.GameUpdate.Register(this, (e) =>
            {
                if (++time == 600)
                {
                    TShock.Players.Where((p) => !string.IsNullOrEmpty(p.GetName()) && Netplay.Clients[p.TPlayer.whoAmI].IsActive).ForEach((p) =>
                    {
                        p.Get<OnlineTime>().Set(d => d.time, d => d.time + 600).Update();
                    });
                    time = 0;
                    TShock.Players.Where((p) => !string.IsNullOrEmpty(p.GetName()) && Netplay.Clients[p.TPlayer.whoAmI].IsActive).ForEach((p) =>
                    {
                        p.Get<DailyOnlineTime>().Set(d => d.time, d => d.time + 600).Update();
                    });
                }
            });

            TShock.RestApi.Register(new SecureRestCommand("/v1/onlinetime/rankboard", (RestRequestArgs args) =>
            {
                int i = 0;
                using (var context = Db.Context<OnlineTime>())
                    return new JArray
                    (
                        context.Config.OrderByDescending(tuple => tuple.time)
                        .AsEnumerable().Select(tuple => new JObject
                        {
                            ["time"] = (long)tuple.time,
                            ["rank"] = ++i,
                            ["name"] = tuple.name
                        }).ToArray()
                    );
            }, new string[] { "onlinetime.rest" }));

            TShock.RestApi.Register(new SecureRestCommand("/v1/dailyonlinetime/rankboard", (RestRequestArgs args) =>
            {
                int i = 0;
                using (var context = Db.Context<DailyOnlineTime>())
                    return new JArray
                    (
                        context.Config.OrderByDescending(tuple => tuple.time)
                        .AsEnumerable().Select(tuple => new JObject
                        {
                            ["time"] = (long)tuple.time,
                            ["rank"] = ++i,
                            ["name"] = tuple.name
                        }).ToArray()
                    );
            }, new string[] { "onlinetime.rest" }));

            TShock.RestApi.Register(new SecureRestCommand("/v1/deathtimes/rankboard", (RestRequestArgs args) =>
            {
                int i = 0;
                using (var context = Db.Context<DeathTimes>())
                    return new JArray
                    (
                        context.Config.OrderByDescending((tuple) => tuple.times)
                        .AsEnumerable().Select((tuple) => new JObject
                        {
                            ["times"] = (long)tuple.times,
                            ["rank"] = ++i,
                            ["name"] = tuple.name
                        }).ToArray()
                    );
            }, new string[] { "deathtimes.rest" }));

            ServerApi.Hooks.ServerJoin.Register(this, (e) =>
            {
                return;
                var p = TShock.Players[e.Who];

                using (var query = Db.Get<InWhitelist>(p.Name))
                    if (!query.Single().status)
                    {
                        p.SilentKickInProgress = true;
                        p.Disconnect("you are not whitelisted.");
                        e.Handled = true;
                    }
            });

            TShock.RestApi.Register(new SecureRestCommand("/v1/whitelist/set", (RestRequestArgs args) =>
            {
                using (var query = Db.Get<InWhitelist>(args.Parameters["name"]))
                    query.Set(w => w.status, bool.Parse(args.Parameters["status"])).Update();
                return new JObject() { ["status"] = "success" };
            }, new string[] { "whitelist.rest" }));

            TShock.RestApi.Register(new SecureRestCommand("/v1/character/query", (RestRequestArgs args) =>
            {
                var usr = CompatHelper.GetUsers().Where((user) => user.Name == args.Parameters["name"]).Single();
                var data = TShock.CharacterDB.GetPlayerData(null, usr.ID);
                using (var query = Db.Get<OnlineTime>(usr.Name))
                    return new JObject
                    {
                        ["group"] = usr.Group,
                        ["ip"] = usr.Ip,
                        ["statLife"] = data.health,
                        ["statLifeMax"] = data.maxHealth,
                        ["statMana"] = data.mana,
                        ["statManaMax"] = data.maxMana,
                        ["questsCompleted"] = data.questsCompleted,
                        ["onlinetime"] = query.Single().time,
                        ["inventory"] = new JArray(data.inventory.Select((item) => new JObject
                        {
                            ["id"] = item.NetId,
                            ["prefix"] = item.PrefixId,
                            ["stack"] = item.Stack
                        })),
                        ["online"] = TShock.Players.Any(p => p.GetName() == usr.Name)
                    };
            }, new string[] { "charcterquery.rest" }));

            var ii = 0;
            TShock.RestApi.Register(new SecureRestCommand("/v1/seconomy/rankboard", (RestRequestArgs args) => new JArray
            (
                SEconomyPlugin.Instance.RunningJournal.BankAccounts.OrderBy((account) => -account.Balance + (ii = 0)).Select((account) => new JObject
                {
                    ["balance"] = (long)account.Balance,
                    ["rank"] = ++ii,
                    ["name"] = account.UserAccountName
                }).ToArray()
            ), new string[] { "sesort" }));
        }
    }
}
