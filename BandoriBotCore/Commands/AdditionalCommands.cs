using BandoriBot.Config;
using BandoriBot.Handler;
using Mirai_CSharp.Models;
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

#pragma warning disable CS0028

namespace BandoriBot.Commands
{
    public static class AdditionalCommands
    {
        public class 执行
        {
            [Superadmin]
            public static void Default(CommandArgs args)
            {
                if (args.Arg == null || args.Arg == "")
                    args.Callback("您未定义指令，请输入正确指令！");
                else
                    args.Callback(string.Join("\n", Configuration.GetConfig<ServerManager>().GetServer(args)
                        .RunCommand(args.Arg)["response"].Select(s => s.ToString())));
            }
        }
        public class 随机禁言
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, long qq)
            {
                //args.Source.Session.SetGroupBanSpeak(args.Source.FromGroup, qq, new TimeSpan(0, 0, new Random().Next(3600 * 24 * 30)));
            }
        }
        public class 绑定
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string username, long qq, string server)
            {
                var binding = Configuration.GetConfig<AccountBinding>().t;

                if (binding.Any((o) => o.username == username || o.qq == qq))
                {
                    args.Callback("你在本客户端已注册了泰拉角色，请输入 泰拉资料 查看你的角色信息");
                    return;
                }

                binding.Add(new Binding
                {
                    username = username,
                    qq = qq,
                    group = Configuration.GetConfig<ServerManager>().servers[server].group
                });
                Configuration.GetConfig<AccountBinding>().Save();

                args.Callback($"你注册的泰拉角色名是{username}=>你的泰拉角色已绑定{qq}可以进入服务器了哦～");
            }
        }
        public class Wiki
        {
            public static void Main(CommandArgs args,string name)
            {
                //https://terraria.fandom.com/zh/wiki/Special:%E6%90%9C%E7%B4%A2?query=木剑
                args.Callback("已经从WiKi上为你找到了【"+name+"】，请点击链接查看哦：\rhttps://terraria.fandom.com/zh/wiki/Special:%E6%90%9C%E7%B4%A2?search=" + System.Web.HttpUtility.UrlEncode(name));
            }
        }
        public class 泰拉注册
        {
            private static readonly HashSet<char> alphabet = new HashSet<char>("1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
            public static void Main(CommandArgs args, string username, string password)
            {
                var binding = Configuration.GetConfig<AccountBinding>().t;
                var serverbinding = Configuration.GetConfig<ServerManager>().bindings;
                var noreg = Configuration.GetConfig<ServerManager>().GetServer(args)?.noRegister;

                if (!string.IsNullOrEmpty(noreg))
                {
                    args.Callback(noreg);
                    return;
                }

                if (args.Source.FromGroup != 0)
                {
                    args.Callback("请私聊我完成注册哦");
                    return;
                }
                if (password.Length < 4)
                {
                    args.Callback("密码不可以小于4位数 ");
                    return;
                }
                if (!password.All(alphabet.Contains))
                {
                    args.Callback("密码不可以为中文，仅限于26个字母和数字");
                    return;
                }
                if (binding.Any((o) => o.username == username && o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group))
                {
                    args.Callback($"{username} already registered.");
                    return;
                }

                if (binding.Any((o) => o.qq == args.Source.FromQQ && o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group))
                {
                    args.Callback("你在本客户端已注册了泰拉角色，请输入 泰拉资料 查看你的角色信息");
                    return;
                }
                var result = Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunCommand($"/user add \"{username}\" \"{password}\" default");
                if ((int)result["status"] != 200)
                {
                    args.Callback(result.ToString());
                    return;
                }

                binding.Add(new Binding
                {
                    username = username,
                    qq = args.Source.FromQQ,
                    group = Configuration.GetConfig<ServerManager>().GetServer(args).group
                });

                Configuration.GetConfig<AccountBinding>().Save();

                args.Callback($"你注册的泰拉角色名是{username}你的泰拉角色已绑定=>{args.Source.FromQQ}=>密码是{password}可以进入服务器了哦～");
            }

            public static void Main(CommandArgs args)
            {
                args.Callback("根据要求格式完成注册:泰拉注册 角色名 密码，例如:泰拉注册 明明 12345，温馨提示：角色名不可为非法符合，密码必须大于5位数，必须用同名的泰拉角色登录服务器哦～");
            }
            public static void Default(CommandArgs args)
            {
                args.Callback("根据要求格式完成注册:泰拉注册 角色名 密码，例如:泰拉注册 明明 12345，温馨提示：角色名不可为非法符合，密码必须大于5位数，必须用同名的泰拉角色登录服务器哦～");
            }
        }

        public class 泰拉在线
        {
            public static void Main(CommandArgs args)
            {
                args.Callback(string.Join("", Configuration.GetConfig<ServerManager>().servers.Select((server) =>
                {
                    try
                    {
                        if (!server.Value.display)
                            if (server.Key == "流光之城")
                            {
                                var online = string.Join("\n", server.Value.RunCommand("/list")["response"].Select(s => s.ToString()));
                                return $"『流光之城』在线({online.Split(',').Count()}/100):\n" + string.Join(" ", online.Split('：')[1].Split(',').Select(s => $"[{s}]")) + "\n";
                            }
                            else
                            {
                                var arr = server.Value.RunRest("/v2/users/activelist")["activeusers"]
                                    .ToString().Split('\t').Where((s) => !string.IsNullOrWhiteSpace(s)).ToArray();
                                return $"『{server.Key}』在线({arr.Length}/100):\n{string.Join(" ", arr.Select(s => $"[{s}]"))}\n";
                            }
                        else
                            return "";
                    }
                    catch
                    {
                        return "";
                    }
                })));
            }
            public static void Main(CommandArgs args, string name)
            {
                switch (name)
                {
                    case "主城":
                        {
                            var server = Configuration.GetConfig<ServerManager>().servers.ToList().Find((KeyValuePair<string, Server> k) => k.Key == "流光之城");
                            var online = string.Join("\n", server.Value.RunCommand("/list")["response"].Select(s => s.ToString()));
                            args.Callback($"『流光之城』在线({online.Split(',').Count()}/100):\n" + string.Join(" ", online.Split('：')[1].Split(',').Select(s => $"[{s}]")) + "\n");
                        }
                        break;
                    case "生存":
                        {
                            args.Callback(string.Join("", Configuration.GetConfig<ServerManager>().servers.Select((server) =>
                            {
                                try
                                {
                                    if (!server.Value.display && server.Key.Contains("生存"))
                                        if (server.Key == "流光之城")
                                        {
                                            var online = string.Join("\n", server.Value.RunCommand("/list")["response"].Select(s => s.ToString()));
                                            return $"『流光之城』在线({online.Split(',').Count()}/100):\n" + string.Join(" ", online.Split('：')[1].Split(',').Select(s => $"[{s}]")) + "\n";
                                        }
                                        else
                                        {
                                            var arr = server.Value.RunRest("/v2/users/activelist")["activeusers"]
                                                .ToString().Split('\t').Where((s) => !string.IsNullOrWhiteSpace(s)).ToArray();
                                            return $"『{server.Key}』在线({arr.Length}/100):\n{string.Join(" ", arr.Select(s => $"[{s}]"))}\n";
                                        }
                                    else
                                        return "";
                                }
                                catch
                                {
                                    return "";
                                }
                            })));
                        }
                        break;
                    case "饥荒":
                        {
                            args.Callback(string.Join("", Configuration.GetConfig<ServerManager>().servers.Select((server) =>
                            {
                                try
                                {
                                    if (!server.Value.display && server.Key.Contains("饥荒"))
                                        if (server.Key == "流光之城")
                                        {
                                            var online = string.Join("\n", server.Value.RunCommand("/list")["response"].Select(s => s.ToString()));
                                            return $"『流光之城』在线({online.Split(',').Count()}/100):\n" + string.Join(" ", online.Split('：')[1].Split(',').Select(s => $"[{s}]")) + "\n";
                                        }
                                        else
                                        {
                                            var arr = server.Value.RunRest("/v2/users/activelist")["activeusers"]
                                                .ToString().Split('\t').Where((s) => !string.IsNullOrWhiteSpace(s)).ToArray();
                                            return $"『{server.Key}』在线({arr.Length}/100):\n{string.Join(" ", arr.Select(s => $"[{s}]"))}\n";
                                        }
                                    else
                                        return "";
                                }
                                catch
                                {
                                    return "";
                                }
                            })));
                        }
                        break;
                    default:
                        {
                            args.Callback(string.Join("", Configuration.GetConfig<ServerManager>().servers.Select((server) =>
                            {
                                try
                                {
                                    if (!server.Value.display)
                                        if (server.Key == "流光之城")
                                        {
                                            var online = string.Join("\n", server.Value.RunCommand("/list")["response"].Select(s => s.ToString()));
                                            return $"『流光之城』在线({online.Split(',').Count()}/100):\n" + string.Join(" ", online.Split('：')[1].Split(',').Select(s => $"[{s}]")) + "\n";
                                        }
                                        else
                                        {
                                            var arr = server.Value.RunRest("/v2/users/activelist")["activeusers"]
                                                .ToString().Split('\t').Where((s) => !string.IsNullOrWhiteSpace(s)).ToArray();
                                            return $"『{server.Key}』在线({arr.Length}/100):\n{string.Join(" ", arr.Select(s => $"[{s}]"))}\n";
                                        }
                                    else
                                        return "";
                                }
                                catch
                                {
                                    return "";
                                }
                            })));
                        }
                        break;
                }
            }
        }

        public class 泰拉玩家
        {
            public static void Main(CommandArgs args)
            {
                args.Callback("当前泰拉服务器玩家列表: \n" + string.Join(" ", Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunRest("/v2/users/list")["users"].Select((user) => $"[{user["name"]}]")));
            }
        }

        public class 泰拉背包
        {
            private static Font font = new Font("微软雅黑", 10, FontStyle.Regular);
            private static string[] textures;
            private static string background, frame;
            private static JObject format;

            static 泰拉背包()
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
            [Permission("terraria.admin")]
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
                    throw new CommandException("该泰拉角色不存在于服务器，请输入 泰拉玩家 查看！", e);
                }
                Bitmap bitmap = new Bitmap(80 * 22, 80 * 12);//1760x960
                Graphics canvas = Graphics.FromImage(bitmap);

                canvas.Clear(Color.White);
                var background = 泰拉背包.background.LoadImage();
                var frame = 泰拉背包.frame.LoadImage();

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
                //args.Callback("test");
            }

            public static void Main(CommandArgs args)
            {
                var player = GetUsername(args);
                if (player == null)
                {
                    args.Callback("你在该客户端尚未注册泰拉角色，请在本客户端注册泰拉角色，再次重发指令哦～");
                    return;
                }
                Main(args, player);
            }
        }

        private static string GetUsername(CommandArgs args) => Configuration.GetConfig<AccountBinding>().t.Where(o => o.qq == args.Source.FromQQ && o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group).FirstOrDefault()?.username?.ToString();

        private static string RankFormat<T>(string title, IEnumerable<T> list, Func<T, string> formatter, int page,
            Func<T, bool> selfpredict, Func<T, string> nameFormatter)
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendJoin("\n", list.Skip(page * 10 - 10).Take(10).Select((t, i) => $"{i + 1}. {nameFormatter(t)}\t{formatter(t)}"));
            int rank = 0;
            var self = list.FirstOrDefault(s =>
            {
                ++rank;
                return selfpredict(s);
            });
            sb.AppendLine();
            if (self != null)
                sb.AppendLine($"{nameFormatter(self)}当前的排行为{rank}");
            sb.Append($"===页码[{page}/{Math.Ceiling((double)(list.Count() / 10))}]");
            return sb.ToString();
        }
        public class 泰拉物品排行
        {
            public static void Main(CommandArgs args, int id, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前物品ID{id}的排行如下: ", Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/v1/itemrank/rankboard?&id={id}").Where(t => (int)t["count"] > 0),
                    rank => $"共拥有{rank["count"]}个", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉物品排行 物品ID 页码");
            }
        }
        public class 泰拉商店
        {
            public static void Main(CommandArgs args,string cmd,int num)
            {
                var server = Configuration.GetConfig<ServerManager>().GetServer(args);
                string res = "还没写，爬！";
                switch (cmd)
                {
                    case "下架":
                        {

                        }
                        break;
                    case "交易列表":
                        {
                            var arr = (server.RunRest("/economy/getshopitems") as JObject)["market"];
                            var list = arr.ToArray();
                            int totalpage = list.Length / 10 + (list.Length % 10 > 0 ? 1 : 0);
                            res = "交易列表：\r";
                            num--;
                            if(num < 0)
                            {
                                num = 0;
                            }
                            for (int i = num * 10; i < ((num + 1) * 10 + 1 < list.Length ? (num + 1) * 10 + 1 : list.Length); i++)
                            {
                                res += $"{i}.物品:{list[i]["ItemID"]}x{list[i]["ItemStack"]} " +
                                    (int.Parse(list[i]["ItemPrefix"].ToString()) == 0 ? "" : $"前缀:{list[i]["ItemPrefix"]}") +
                                    $"价格:{list[i]["Price"]} 出售者:{list[i]["Owner"]}\r";
                            }
                            res += $"==页码[{num + 1}/{totalpage}]";
                        }
                        break;
                    case "商店列表":
                        {
                            var arr = (server.RunRest("/economy/getshopitems") as JObject)["shop"];
                            var list = arr.ToArray();
                            int totalpage = list.Length / 10 + (list.Length % 10 > 0 ? 1 : 0);
                            res = "商店列表：\r";
                            num--;
                            if (num < 0)
                            {
                                num = 0;
                            }
                            for (int i = num * 10; i < ((num + 1) * 10 + 1 < list.Length ? (num + 1) * 10 + 1 : list.Length); i++)
                            {
                                res += $"{i}.物品:{list[i]["ItemID"]}x{list[i]["ItemStack"]} " +
                                    (int.Parse(list[i]["ItemPrefix"].ToString()) == 0 ? "" : $"前缀:{list[i]["ItemPrefix"]}") +
                                    $"价格:{list[i]["Price"]}\r";
                            }
                            res += $"==页码[{num + 1}/{totalpage}]";
                        }
                        break;
                    case "购买":
                        {

                        }
                        break;
                    default:
                        {
                            res = "没这功能，爬！";
                        }
                        break;
                }
                args.Callback(res);
            }
            public static void 上架(CommandArgs args,int id,int stack,int price)
            {
                string res = "还没写，爬！";
                args.Callback(res);
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式：\r" +
                    "泰拉商店 商店列表 页码\r" +
                    "泰拉商店 交易列表 页码\r" +
                    "泰拉商店 下架 编号\r" +
                    "泰拉商店 上架 物品ID 数量 价格\r" +
                    "泰拉商店 购买 序号");
            }
        }
        public class 泰拉财富排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前财富排行如下: ", Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/economy/getmoneyrank")["response"],
                    rank => $"共拥有${rank["Value"]}", page, rank => rank.Value<string>("Key") == name, rank => $"[{rank["Key"]}]"));

            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉财富排行 页码");
            }
        }

        public class 泰拉重生排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前重生次数排行如下: ", Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/v1/deathtimes/rankboard"),
                    rank => $"共计重生{rank["times"]}次", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉重生排行 页码");
            }
        }

        public class 泰拉渔夫排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前渔夫任务排行如下: ", Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/v1/questrank/rankboard"),
                    rank => $"任务完成{rank["times"]}次", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉渔夫排行 页码");
            }
        }

        public class 泰拉在线排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前在线排行如下: ", Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/v1/onlinetime/rankboard"),
                    rank => $"共计在线{(int)rank["time"] / 3600}分钟", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉在线排行 页码");
            }
        }
        public class 泰拉每日在线排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前在线排行如下: ", Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/v1/dailyonlinetime/rankboard"),
                    rank => $"共计在线{(int)rank["time"] / 3600}分钟", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉每日在线排行 页码");
            }
        }

        public class 封ip
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string ip)
            {
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban addip {ip}");
                args.Callback(string.Format(GlobalConfiguration.Global.func5Info, ip));
            }
        }

        public class 解ip
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string ip)
            {
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban delip {ip}");
                args.Callback(string.Format(GlobalConfiguration.Global.func61Info, ip));
            }
        }

        public class 封
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string name)
            {
                try
                {
                    Configuration.GetConfig<Blacklist>().hash.Add(Configuration.GetConfig<AccountBinding>().t.Where((o) => o.username == name && o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group).FirstOrDefault().qq);
                    Configuration.GetConfig<Blacklist>().Save();
                }
                catch (NullReferenceException)
                {
                    throw new CommandException("该泰拉玩家未在本客户端绑定qq号");
                }
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban add {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func5Info, name));

                //args.Source.Session.SetGroupMemberRemove(args.Source.FromGroup,
                //    (long)Configuration.GetConfig<AccountBinding>().t.Where((o) => o.username == name).Single().qq);
            }
        }

        public class 解
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string name)
            {
                try
                {
                    Configuration.GetConfig<Blacklist>().hash.Remove((long)Configuration.GetConfig<AccountBinding>().t.Where((o) => o.username == name && o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group).FirstOrDefault().qq);
                    Configuration.GetConfig<Blacklist>().Save();
                }
                catch (NullReferenceException)
                {
                    throw new CommandException("该泰拉玩家未在本客户端绑定qq号");
                }
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/ban del {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func61Info, name));
            }
        }

        public class 重置
        {
            [Superadmin]
            public static void Main(CommandArgs args, string name)
            {
                Configuration.GetConfig<ServerManager>().GetServer(args).RunCommand($"/user del {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func62Info, name));
            }
        }

        public class 解绑
        {
            [Superadmin]
            public static void Main(CommandArgs args, string name)
            {
                var lst = Configuration.GetConfig<AccountBinding>().t;
                var lst2 = new List<Binding>();
                foreach (var obj in lst)
                    if ((string)obj.username != name || (int)obj.group != Configuration.GetConfig<ServerManager>().GetServer(args).group)
                        lst2.Add(obj);

                Configuration.GetConfig<AccountBinding>().t = lst2;
                Configuration.GetConfig<AccountBinding>().Save();
                args.Callback(string.Format(GlobalConfiguration.Global.func63Info, name));
            }
        }

        public class 泰拉切换
        {
            public static void Main(CommandArgs args, string name)
            {
                if (!Configuration.GetConfig<ServerManager>().servers.ContainsKey(name))
                {
                    args.Callback("该服务器不存在，请输入 服务器列表 查看可切换的服务器哦！");
                    return;
                }

                /*foreach (var pair in Configuration.GetConfig<ServerManager>().servers)
                {
                    var acc = (string)Configuration.GetConfig<AccountBinding>().t.Where(o => o.group == pair.Value.group && o.qq == args.Source.FromQQ).FirstOrDefault()?.username;
                    if (acc == null) continue;
                    pair.Value.RunCommand($"/wl {HttpUtility.UrlEncode(acc)} {name == pair.Key}");
                    //pair.Value.RunRest($"/v1/whitelist/set?name={HttpUtility.UrlEncode(acc)}&status={name == pair.Key}");
                }*/

                Configuration.GetConfig<ServerManager>().SwitchTo(args.Source.FromQQ, name);
                
                if (Configuration.GetConfig<AccountBinding>().t.Any(o => o.qq == args.Source.FromQQ && o.group == Configuration.GetConfig<ServerManager>().servers.FirstOrDefault(s => s.Key == name).Value.group))
                {
                    args.Callback($"你的客户端和角色已切换到{name}，可以进入泰拉服务器了");
                }
                else
                {
                    args.Callback($"你的客户端已切换到{name}，请在本客户端注册你的泰拉角色才能进入服务器哦～请私聊我输入:泰拉注册");
                }
            }
        }
        private static string GetCurrentServer(string name)
        {
            var servers= Configuration.GetConfig<ServerManager>().servers.Select((server) =>
            {
                if (server.Value.RunRest("/v2/users/activelist")["activeusers"].ToString().Contains(name))
                    return server.Key;
                else
                    return "";
            }).ToArray();
            foreach(var s in servers)
            {
                if (s != "")
                    return s;
            }
            return "";
        }
        public class test
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args,int id,string name)
            {
                var server = Configuration.GetConfig<ServerManager>().GetServer(args);
                args.Callback(server.RunRest("/v1/itemrank/rankboard?&id=" + 71).Where(t => t["name"].ToString() == name).FirstOrDefault().ToString());
            }
        }
        public class 泰拉资料
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args,string type,long qq)
            {
                if (type != "QQ") return;
                var account = Configuration.GetConfig<AccountBinding>().t.Where(o => (o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group || Configuration.GetConfig<ServerManager>().GetServerName(args) == "流光之城") && o.qq == qq).FirstOrDefault();
                if (account == null)
                {
                    throw new CommandException("未找到该玩家资料！");
                }
                else
                {
                    JObject data;
                    try
                    {
                        data = Configuration.GetConfig<ServerManager>()
                        .GetServer(args).RunRest($"/v1/character/query?name={HttpUtility.UrlEncode(account.username)}") as JObject;
                    }
                    catch (CommandException e)
                    {
                        throw new CommandException("该泰拉角色不存在于服务器，请输入 泰拉玩家 查看！", e);
                    }
                    JObject bank = null;
                    try
                    {
                        bank = Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/economy/getplayermoney?player={HttpUtility.UrlEncode(account.username)}") as JObject;
                    }
                    catch { }
                    args.Callback($"这是泰拉玩家[{account.username}]的资料\n" +
                        $"QQ: {account.qq}\n" +
                        //$"积分: {data["ip"]}\n" +；//$"神晶: {data["ip"]}\n" +
                        $"权限: {data["group"]}\n" +
                        //$"等级: {data["ip"]}\n" +
                        //$"经验: {data["ip"]}\n" +；//$"财富: {data["ip"]}\n" +
                        $"货币：{Utils.GetMoney(Configuration.GetConfig<ServerManager>().GetServer(args), account.username)}\r" +
                        $"生命：{data["statLife"]}/{data["statLifeMax"]}\n" +
                        $"法力：{data["statMana"]}/{data["statManaMax"]}\n" +
                        ((bank != null && bank["status"].ToString() == "200") ? $"经济：{bank["money"]}$\n" : "") +
                        $"钓鱼任务完成次数: {data["questsCompleted"]}\n" +
                        //$"今日总在线时长:功能未实现\n" +
                        //$"本期PE在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                        //$"本期PC在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                        $"本期总在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                    //$"当前服务器阶段: {((bool)data["online"] ? "肉前阶段" : "肉后阶段" : "巨人前阶段" : "巨人后阶段" : "四柱阶段" : "月后阶段")}");
                    //$"状态: {((bool)data["online"] ? $"在线 ({GetCurrentServer(account.username)})" : "离线")}");
                    $"状态: {(Utils.PlayerOnline(account.username) ? $"在线 ({Utils.GetOnlineServer(account.username)})" : "离线")}");
                }
            }
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string name)
            {
                /*bool isqq = long.TryParse(name, out long qq);
                Binding account = null;
                if (isqq)
                {
                    account = Configuration.GetConfig<AccountBinding>().t.Where(o => (o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group || Configuration.GetConfig<ServerManager>().GetServerName(args) == "流光之城") && o.qq == qq).FirstOrDefault();
                }
                else
                {
                    account = Configuration.GetConfig<AccountBinding>().t.Where(o => (o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group || Configuration.GetConfig<ServerManager>().GetServerName(args) == "流光之城") && o.username == name).FirstOrDefault();
                }*/
                Binding account = Configuration.GetConfig<AccountBinding>().t.Where(o => (o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group || Configuration.GetConfig<ServerManager>().GetServerName(args) == "流光之城") && o.username == name).FirstOrDefault();
                if (account == null)
                {
                    throw new CommandException("未找到该玩家资料！");
                }
                else
                {
                    JObject data;
                    try
                    {
                        data = Configuration.GetConfig<ServerManager>()
                        .GetServer(args).RunRest($"/v1/character/query?name={HttpUtility.UrlEncode(account.username)}") as JObject;
                    }
                    catch (CommandException e)
                    {
                        throw new CommandException("该泰拉角色不存在于服务器，请输入 泰拉玩家 查看！", e);
                    }
                    JObject bank = null;
                    try
                    {
                        bank = Configuration.GetConfig<ServerManager>().GetServer(args).RunRest($"/economy/getplayermoney?player={HttpUtility.UrlEncode(account.username)}") as JObject;
                    }
                    catch { }
                    args.Callback($"这是泰拉玩家[{account.username}]的资料\n" +
                        $"QQ: {account.qq}\n" +
                        //$"积分: {data["ip"]}\n" +；//$"神晶: {data["ip"]}\n" +
                        $"权限: {data["group"]}\n" +
                        //$"等级: {data["ip"]}\n" +
                        //$"经验: {data["ip"]}\n" +；//$"财富: {data["ip"]}\n" +
                        $"货币：{Utils.GetMoney(Configuration.GetConfig<ServerManager>().GetServer(args),account.username)}\r"+
                        $"生命：{data["statLife"]}/{data["statLifeMax"]}\n" +
                        $"法力：{data["statMana"]}/{data["statManaMax"]}\n" +
                        ((bank!=null&&bank["status"].ToString()=="200")?$"经济：{bank["money"]}$\n":"")+
                        $"钓鱼任务完成次数: {data["questsCompleted"]}\n" +
                        //$"今日总在线时长:功能未实现\n" +
                        //$"本期PE在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                        //$"本期PC在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                        $"本期总在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                    //$"当前服务器阶段: {((bool)data["online"] ? "肉前阶段" : "肉后阶段" : "巨人前阶段" : "巨人后阶段" : "四柱阶段" : "月后阶段")}");
                    //$"状态: {((bool)data["online"] ? $"在线 ({GetCurrentServer(account.username)})" : "离线")}");
                    $"状态: {(Utils.PlayerOnline(account.username) ? $"在线 ({Utils.GetOnlineServer(account.username)})" : "离线")}");
                }
            }
            
            public static void Main(CommandArgs args)
            {
                var player = Configuration.GetConfig<AccountBinding>().t.Where(o => o.qq == args.Source.FromQQ && o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group).FirstOrDefault()?.username?.ToString();
                if (player == null)
                {
                    args.Callback("你在该客户端尚未注册泰拉角色，请在本客户端注册泰拉角色，再次重发指令哦～");
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
                args.Callback($"恭喜玩家获得 [{name}]×{a} 消耗4积分,还剩{Configuration.GetConfig<ScoreConfig>()[args.Source.FromQQ] - 4}积分");
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
                //args.Callback(string.Join("\n", Configuration.GetConfig<ServerManager>().servers.Where(pair => pair.Value.display).Select(pair => pair.Key)));
                args.Callback(string.Join("\n", Configuration.GetConfig<ServerManager>().servers.Select(pair => pair.Key)));
            }
        }

        public class 加入黑名单
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, long qq)
            {
                if(Configuration.GetConfig<Admin >().hash.ToList().FindAll((long num)=>num==qq).Count == 0)
                {
                    Configuration.GetConfig<Blacklist>().hash.Add(qq);
                    Configuration.GetConfig<Blacklist>().Save();
                    args.Callback(qq + "已加入黑名单");
                }
                else
                {
                    args.Callback(qq + "为管理员，无法加入黑名单");
                }
            }
        }
        public class 移除黑名单
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, long qq)
            {
                Configuration.GetConfig<Blacklist>().hash.Remove(qq);
                Configuration.GetConfig<Blacklist>().Save();
                args.Callback(qq + "已移除黑名单");
            }
        }
        public class 黑名单
        {
            public static void Main(CommandArgs args)
            {
                args.Callback("命令列表：\n添加黑名单 QQ\n移除黑名单\n黑名单列表 页码\n查黑 QQ");
            }
        }
        public class 黑名单列表
        {
            public static void Main(CommandArgs args)
            {
                int i = 0;
                args.Callback("黑名单列表 页码");
                //args.Callback("黑名单列表如下\n" + string.Join("\n", Configuration.GetConfig<Blacklist>().hash.Select(qq => $"{++i}. {qq}")));
            }
            public static void Main(CommandArgs args, int page)
            {

                string info = "黑名单列表如下";
                //args.Callback("黑名单列表如下\n" + string.Join("\n", Configuration.GetConfig<Blacklist>().hash.Select(qq => $"{++i}. {qq}")));
                var list = Configuration.GetConfig<Blacklist>().hash.ToList();
                for (int i = Math.Min((page - 1), list.Count / 20) * 20; i < Math.Min(page * 20, list.Count); i++)
                {
                    info += $"\n{i + 1}. {list[i]}";
                }
                info += $"\n第[{Math.Min(list.Count / 20 + 1, page)}/{list.Count / 20 + 1}]页";
                args.Callback(info);
            }
        }
        public class 查黑
        {
            public static void Main(CommandArgs args,long QQ)
            {
                int i = 0;
                var list = string.Join("\n", Configuration.GetConfig<Blacklist>().hash.Select(qq => $"{++i}. {qq}"));
                args.Callback((list.Contains(QQ.ToString()) ? "该用户在黑名单内" : "该用户不在黑名单内"));
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
