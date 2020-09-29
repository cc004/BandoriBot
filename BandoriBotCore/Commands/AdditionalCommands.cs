using BandoriBot.Config;
using Native.Csharp.App.Terraria;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BandoriBot.Commands
{
    public static class AdditionalCommands
    {
        public class 执行
        {
            [Admin]
            public static void Main(CommandArgs args, string cmd)
            {
                args.Callback(string.Join("\n", Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunCommand(cmd.Replace("\\", " "))["response"].Select(s => s.ToString())));
            }
        }
        public class 随机禁言
        {
            [Admin]
            public static void Main(CommandArgs args, long qq)
            {
                //Common.CqApi.SetGroupBanSpeak(args.Source.FromGroup, qq, new TimeSpan(0, 0, new Random().Next(3600 * 24 * 30)));
            }
        }
        public class 绑定
        {
            [Admin]
            public static void Main(CommandArgs args, string username, long qq, string server)
            {
                var binding = Configuration.GetConfig<AccountBinding>().list;

                if (binding.Any((o) => (string)o["username"] == username || (long)o["qq"] == qq))
                {
                    args.Callback("已经绑定过了!");
                    return;
                }

                binding.Add(new JObject
                {
                    ["username"] = username,
                    ["qq"] = qq,
                    ["group"] = Configuration.GetConfig<ServerManager>().servers[server].group
                });
                Configuration.GetConfig<AccountBinding>().Save();

                args.Callback($"已绑定 {username}<=>{qq}");
            }
        }
        public class 注册
        {
            public static void Main(CommandArgs args, string username, string password)
            {
                var binding = Configuration.GetConfig<AccountBinding>().list;
                var serverbinding = Configuration.GetConfig<ServerManager>().bindings;

                if (binding.Any((o) => (string)o["username"] == username && (int)o["group"] == Configuration.GetConfig<ServerManager>().GetServer(args).group))
                {
                    args.Callback("已经绑定过了!");
                    return;
                }

                var result = Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunCommand($"/user add \"{username}\" \"{password}\" default");
                if ((int)result["status"] != 200)
                {
                    args.Callback(result.ToString());
                    return;
                }

                binding.Add(new JObject
                {
                    ["username"] = username,
                    ["qq"] = args.Source.FromQQ,
                    ["group"] = Configuration.GetConfig<ServerManager>().GetServer(args).group
                });

                Configuration.GetConfig<AccountBinding>().Save();

                args.Callback($"已绑定 {username}<=>{args.Source.FromQQ}");
            }

            public static void Main(CommandArgs args)
            {
                args.Callback("注册 [用户名] [密码]");
            }
        }

        public class 泰拉在线
        {
            public static void Main(CommandArgs args)
            {
                args.Callback(string.Join("\n", Configuration.GetConfig<ServerManager>().servers.Select((server) =>
                {
                    var arr = server.Value.RunRest("/v2/users/activelist")["activeusers"]
                        .ToString().Split('\t').Where((s) => !string.IsNullOrWhiteSpace(s)).ToArray();
                    return $"当前{server.Key}在线的玩家有{arr.Length}个:\n{string.Join(" ", arr.Select(s => $"[{s}]"))}";
                })));

            }
        }

        public class 用户
        {
            private static Font font = new Font("微软雅黑", 10, FontStyle.Regular);
            private static string[] textures;
            private static string background, frame;
            private static JObject format;

            static 用户()
            {
                background = Path.Combine("", @$"textures\background.png");
                frame = Path.Combine("", @$"textures\frame.png");
                format = JObject.Parse(File.ReadAllText(Path.Combine("", @$"textures\format.json")));
                textures = new string[5045];
                for (int i = 0; i < 5045; ++i)
                    textures[i] = Path.Combine("", @$"textures\{i}.png");
            }

            #region bullshit
            private static short FromNetId(short id)
            {
                switch (id)
                {
                    case -48:
                        return 3480;
                    case -47:
                        return 3481;
                    case -46:
                        return 3482;
                    case -45:
                        return 3483;
                    case -44:
                        return 3484;
                    case -43:
                        return 3485;
                    case -42:
                        return 3486;
                    case -41:
                        return 3487;
                    case -40:
                        return 3488;
                    case -39:
                        return 3489;
                    case -38:
                        return 3490;
                    case -37:
                        return 3491;
                    case -36:
                        return 3492;
                    case -35:
                        return 3493;
                    case -34:
                        return 3494;
                    case -33:
                        return 3495;
                    case -32:
                        return 3496;
                    case -31:
                        return 3497;
                    case -30:
                        return 3498;
                    case -29:
                        return 3499;
                    case -28:
                        return 3500;
                    case -27:
                        return 3501;
                    case -26:
                        return 3502;
                    case -25:
                        return 3503;
                    case -24:
                        return 3769;
                    case -23:
                        return 3768;
                    case -22:
                        return 3767;
                    case -21:
                        return 3766;
                    case -20:
                        return 3765;
                    case -19:
                        return 3764;
                    case -18:
                        return 3504;
                    case -17:
                        return 3505;
                    case -16:
                        return 3506;
                    case -15:
                        return 3507;
                    case -14:
                        return 3508;
                    case -13:
                        return 3509;
                    case -12:
                        return 3510;
                    case -11:
                        return 3511;
                    case -10:
                        return 3512;
                    case -9:
                        return 3513;
                    case -8:
                        return 3514;
                    case -7:
                        return 3515;
                    case -6:
                        return 3516;
                    case -5:
                        return 3517;
                    case -4:
                        return 3518;
                    case -3:
                        return 3519;
                    case -2:
                        return 3520;
                    case -1:
                        return 3521;
                    default:
                        return id;
                }
            }

            #endregion
            [Admin]
            public static void Main(CommandArgs args, string name)
            {
                JObject data;
                try
                {
                    data = Configuration.GetConfig<ServerManager>()
                        .GetServer(args).RunRest($"/v1/character/query?name={HttpUtility.UrlEncode(name)}") as JObject;
                }
                catch (CommandException e)
                {
                    throw new CommandException("用户不存在！", e);
                }
                Bitmap bitmap = new Bitmap(80 * 22, 80 * 12);
                Graphics canvas = Graphics.FromImage(bitmap);

                canvas.Clear(Color.White);
                var background = 用户.background.LoadImage();
                var frame = 用户.frame.LoadImage();

                canvas.DrawImage(background, new Rectangle(0, 0, 80 * 22, 80 * 12), new Rectangle(0, 0, background.Width, background.Height), GraphicsUnit.Pixel);

                int pos = 0;
                foreach (var item in data["inventory"])
                {
                    var id = FromNetId((short)item["id"]);
                    var tex = textures[id].LoadImage();
                    var x = (pos % 22) * 80;
                    var y = (pos / 22) * 80;
                    var height = (int)format[id.ToString()]["height"];
                    var width = (int)format[id.ToString()]["width"];
                    if (tex == null)
                    {
                        //canvas.DrawString($"id{item["id"]}", font, Brushes.Black, (pos % 20) * 32f, (pos / 20) * 48f + 128f);
                    }
                    else
                    {
                        canvas.DrawImage(frame, x + 10, y + 10, new Rectangle(0, 0, 60, 60), GraphicsUnit.Pixel);
                        canvas.DrawImage(tex, x + 40 - width / 2, y + 40 - height / 2, new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
                    }
                    tex.Dispose();
                    canvas.DrawString($"x{item["stack"]}", font, Brushes.Black, x + 10, y + 10);
                    ++pos;
                }

                background.Dispose();
                frame.Dispose();
                args.Callback(Utils.GetImageCode(bitmap));
            }

            public static void Main(CommandArgs args)
            {
                args.Callback("用户列表: \n" + string.Join(" ", Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunRest("/v2/users/list")["users"].Select((user) => $"[{user["name"]}]")));
            }
        }

        public class 物品排行
        {
            public static void Main(CommandArgs args, int id, int page)
            {
                args.Callback($"当前物品ID{id}的排行如下: \n" + string.Join("\n", Configuration.GetConfig<ServerManager>()
                    .GetServer(args).RunRest($"/v1/itemrank/rankboard?&id={id}").Skip(page * 10 - 10).Take(10)
                    .Select((rank) => $"{rank["rank"]}. [{rank["name"]}]\t拥有{rank["count"]}个")));
            }
        }

        public class 财富排行
        {
            public static void Main(CommandArgs args, int page)
            {
                args.Callback("当前财富排行如下: \n" + string.Join("\n", Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunRest("/v1/seconomy/rankboard").Skip(page * 10 - 10).Take(10)
                    .Select((rank) => $"{rank["rank"]}. [{rank["name"]}]\t${rank["balance"]}")));
            }
        }

        public class 死亡排行
        {
            public static void Main(CommandArgs args, int page)
            {
                args.Callback("当前死亡次数排行如下: \n" + string.Join("\n", Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunRest("/v1/deathtimes/rankboard").Skip(page * 10 - 10).Take(10)
                    .Select((rank) => $"{rank["rank"]}. [{rank["name"]}]\t$共计死亡{rank["times"]}次")));
            }
        }

        public class 渔夫排行
        {
            public static void Main(CommandArgs args, int page)
            {
                args.Callback("当前渔夫任务排行如下: \n" + string.Join("\n", Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunRest("/v1/questrank/rankboard").Skip(page * 10 - 10).Take(10)
                    .Select((rank) => $"{rank["rank"]}. [{rank["name"]}]\t渔夫任务完成{rank["times"]}次")));
            }
        }
        
        public class 在线排行
        {
            public static void Main(CommandArgs args, int page)
            {
                args.Callback("当前在线排行如下: \n" + string.Join("\n", Configuration.GetConfig<ServerManager>()
                    .GetServer(args).RunRest("/v1/onlinetime/rankboard").Skip(page * 10 - 10).Take(10)
                    .Select((rank) => $"{rank["rank"]}. {rank["name"]}\t共计在线{(int)rank["time"] / 3600}分钟")));
            }
        }

        public class 封ip
        {
            [Admin]
            public static void Main(CommandArgs args, string ip)
            {
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban addip {ip}");
                args.Callback(string.Format(GlobalConfiguration.Global.func5Info, ip));
            }
        }

        public class 解ip
        {
            [Admin]
            public static void Main(CommandArgs args, string ip)
            {
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban delip {ip}");
                args.Callback(string.Format(GlobalConfiguration.Global.func61Info, ip));
            }
        }

        public class 封
        {
            [Admin]
            public static void Main(CommandArgs args, string name)
            {
                try
                {
                    Configuration.GetConfig<Blacklist>().hash.Add((long)Configuration.GetConfig<AccountBinding>().list.Where((o) => (string)o["username"] == name && (int)o["group"] == Configuration.GetConfig<ServerManager>().GetServer(args).group).FirstOrDefault()["qq"]);
                    Configuration.GetConfig<Blacklist>().Save();
                }
                catch (NullReferenceException)
                {
                    throw new CommandException("该用户未绑定qq");
                }
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban add {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func5Info, name));

                //Common.CqApi.SetGroupMemberRemove(args.Source.FromGroup,
                //    (long)Configuration.GetConfig<AccountBinding>().list.Where((o) => (string)o["username"] == name).Single()["qq"]);
            }
        }

        public class 解
        {
            [Admin]
            public static void Main(CommandArgs args, string name)
            {
                try
                {
                    Configuration.GetConfig<Blacklist>().hash.Remove((long)Configuration.GetConfig<AccountBinding>().list.Where((o) => (string)o["username"] == name && (int)o["group"] == Configuration.GetConfig<ServerManager>().GetServer(args).group).FirstOrDefault()["qq"]);
                    Configuration.GetConfig<Blacklist>().Save();
                }
                catch (NullReferenceException)
                {
                    throw new CommandException("该用户未绑定qq");
                }
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban del {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func61Info, name));
            }
        }

        public class 重置
        {
            [Admin]
            public static void Main(CommandArgs args, string name)
            {
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/user del {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func62Info, name));
            }
        }

        public class 解绑
        {
            [Admin]
            public static void Main(CommandArgs args, string name)
            {
                var lst = Configuration.GetConfig<AccountBinding>().list;
                var lst2 = new List<JObject>();
                foreach (var obj in lst)
                    if ((string)obj["username"] != name || (int)obj["group"] != Configuration.GetConfig<ServerManager>().GetServer(args).group)
                        lst2.Add(obj);
                
                Configuration.GetConfig<AccountBinding>().list = lst2;
                Configuration.GetConfig<AccountBinding>().Save();
                args.Callback(string.Format(GlobalConfiguration.Global.func63Info, name));
            }
        }

        public class 切换
        {
            public static void Main(CommandArgs args, string name)
            {
                if (!Configuration.GetConfig<ServerManager>().servers.ContainsKey(name))
                {
                    args.Callback("服务器不存在！");
                    return;
                }

                foreach (var pair in Configuration.GetConfig<ServerManager>().servers)
                {
                    var acc = (string)Configuration.GetConfig<AccountBinding>().list.Where(o => (int)o["group"] == pair.Value.group && (long)o["qq"] == args.Source.FromQQ).FirstOrDefault()?["username"];
                    if (acc == null) continue;
                    pair.Value.RunRest($"/v1/whitelist/set?name={HttpUtility.UrlEncode(acc)}&status={name == pair.Key}");
                }
                
                Configuration.GetConfig<ServerManager>().SwitchTo(args.Source.FromQQ, name);
                args.Callback("切换到" + name);
            }
        }

        public class 泰拉资料
        {
            [Admin]
            public static void Main(CommandArgs args, string name)
            {
                JObject data;
                try
                {
                    data = Configuration.GetConfig<ServerManager>()
                    .GetServer(args).RunRest($"/v1/character/query?name={HttpUtility.UrlEncode(name)}") as JObject;
                }
                catch (CommandException e)
                {
                    throw new CommandException("用户不存在！", e);
                }

                args.Callback($"这是玩家[{name}]的资料\n" +
                    $"QQ: {Configuration.GetConfig<AccountBinding>().list.Where(o => (int)o["group"] == Configuration.GetConfig<ServerManager>().GetServer(args).group && (string)o["username"] == name).FirstOrDefault()?["qq"]?.ToString() ?? "未绑定"}\n" +
                    //$"IP: {data["ip"]}\n" +
                    $"生命: {data["statLife"]}/{data["statLifeMax"]}\n" +
                    $"法力: {data["statMana"]}/{data["statManaMax"]}\n" +
                    $"钓鱼任务完成次数: {data["questsCompleted"]}\n" +
                    //$"今日在线时长:功能未实现\n" +
                    $"总在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                    $"权限: {data["group"]}\n" +
                    $"状态: {((bool)data["online"] ? "在线" : "离线")}");
            }

            public static void Main(CommandArgs args)
            {
                var player = Configuration.GetConfig<AccountBinding>().list.Where(o => (long)o["qq"] == args.Source.FromQQ && (int)o["group"] == Configuration.GetConfig<ServerManager>().GetServer(args).group).FirstOrDefault()?["username"]?.ToString();
                if (player == null)
                {
                    args.Callback("你还没有注册账号");
                    return;
                }
                Main(args, player);
            }
        }
        public class 泰拉祝福
        {
            public static void Main(CommandArgs args)
            {
                var s = Configuration.GetConfig<ScoreConfig>()[args.Source.FromQQ];
                if (s < 4) return;
                Configuration.GetConfig<ScoreConfig>()[args.Source.FromQQ] -= 4;
                var list = Configuration.GetConfig<RandomList>().json as JArray;
                var rnd = list[new Random().Next(0, list.Count)];
                var name = (string)rnd["name"];
                var a = new Random().Next((int)rnd["min"], (int)rnd["max"] + 1);
                args.Callback($"恭喜玩家获得 {name}×{a} 消耗4积分,还剩{Configuration.GetConfig<ScoreConfig>()[args.Source.FromQQ] - 4}积分");
                throw new NotImplementedException();
            }
        }
        public class 泰拉签到
        {
            public static void Main(CommandArgs args)
            {
                if (Configuration.GetConfig<DailyConfig>().hash.Contains(args.Source.FromQQ)) return;
                var a = new Random().Next(7, 15);
                args.Callback($"玩家{args.Source.GetName()}获得{a}积分随机，积分每日重置请尽量用完哦（用于抽奖，兑换建筑材料）");
                Configuration.GetConfig<DailyConfig>().hash.Add(args.Source.FromQQ);
                Configuration.GetConfig<ScoreConfig>()[args.Source.FromQQ] += a;
                Configuration.GetConfig<ScoreConfig>().Save();
            }
        }
        public class 兑换龙珠
        {
            const string list = "①②③④⑤⑥⑦⑨⑩";
            public static void Main(CommandArgs args)
            {
                var s = Configuration.GetConfig<ScoreConfig>()[args.Source.FromQQ];
                if (s < 5) return;
                Configuration.GetConfig<ScoreConfig>()[args.Source.FromQQ] -= 5;
                var a = list[new Random().Next(list.Length)];
                args.Callback($"玩家[{Utils.GetName(args.Source)}]获得龙珠{a}消耗了5积分");
                Configuration.GetConfig<LZConfig>()[args.Source.FromQQ] += a;
            }
        }

        public class 龙珠背包
        {
            public static void Main(CommandArgs args)
            {
                args.Callback($"玩家[{Utils.GetName(args.Source)}]的龙珠背包有{Configuration.GetConfig<LZConfig>()[args.Source.FromQQ]}");
            }
        }

        public class 开启前缀检测
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func1Enabled = true;
                Configuration.GetConfig<GlobalConfiguration>().Save();
            }
        }
        public class 关闭前缀检测
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func1Enabled = false;
                Configuration.GetConfig<GlobalConfiguration>().Save();
            }
        }
        public class 开启自动清人
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func2Enabled = true;
                Configuration.GetConfig<GlobalConfiguration>().Save();
            }
        }
        public class 关闭自动清人
        {
            public static void Main(CommandArgs args)
            {
                GlobalConfiguration.Global.func2Enabled = false;
                Configuration.GetConfig<GlobalConfiguration>().Save();
            }
        }
        public class 服务器列表
        {
            public static void Main(CommandArgs args)
            {
                args.Callback(string.Join("\n", Configuration.GetConfig<ServerManager>().servers.Select(pair => pair.Key)));
            }
        }

        public class 加入黑名单
        {
            public static void Main(CommandArgs args, long qq)
            {
                Configuration.GetConfig<Blacklist>().hash.Add(qq);
                Configuration.GetConfig<Blacklist>().Save();
                args.Callback(qq + "已加入黑名单");
            }
        }
        public class 移除黑名单
        {
            public static void Main(CommandArgs args, long qq)
            {
                Configuration.GetConfig<Blacklist>().hash.Remove(qq);
                Configuration.GetConfig<Blacklist>().Save();
                args.Callback(qq + "已移除黑名单");
            }
        }
        public class 黑名单列表
        {
            public static void Main(CommandArgs args)
            {
                int i = 0;
                args.Callback("黑名单类别如下\n" + string.Join("\n", Configuration.GetConfig<Blacklist>().hash.Select(qq => $"{++i}. {qq}")));
            }
        }

        public class saveall
        {
            public static void Main(CommandArgs args)
            {
                //Configuration.SaveAll();
                args.Callback("saved all.");
            }
        }
    }
}
