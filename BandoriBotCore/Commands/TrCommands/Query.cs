using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using BandoriBot.Config;
using BandoriBot.Terraria;
using Newtonsoft.Json.Linq;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 泰拉资料
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string type, long qq)
            {
                if (type != "QQ") return;
                var account = Configuration.GetConfig<AccountBinding>().t.Where(o => (o.group == Configuration.GetConfig<ServerManager>().GetServer(args).group || Configuration.GetConfig<ServerManager>().GetServerName(args) == "流光之城") && o.qq == qq).FirstOrDefault();
                if (account == null)
                {
                    throw new CommandException("未找到该玩家资料！");
                }
                Main(args, account.username);
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
                var online = manager.GetOnlineServer(account.username);
                args.Callback($"这是泰拉玩家[{account.username}]的资料\n" +
                    $"QQ: {account.qq}\n" +
                    //$"积分: {data["ip"]}\n" +；//$"神晶: {data["ip"]}\n" +
                    $"权限: {data["group"]}\n" +
                    //$"等级: {data["ip"]}\n" +
                    //$"经验: {data["ip"]}\n" +；//$"财富: {data["ip"]}\n" +
                    $"货币：{GetServer(args).GetMoney(account.username)}\r" +
                    $"生命：{data["statLife"]}/{data["statLifeMax"]}\n" +
                    $"法力：{data["statMana"]}/{data["statManaMax"]}\n" +
                    ((bank != null && bank["status"].ToString() == "200") ? $"经济：{bank["money"]}$\n" : "") +
                    $"钓鱼任务完成次数: {data["questsCompleted"]}\n" +
                    $"今日总在线时长:{(int)data["daily"]}分钟\n" +
                    $"本期在线时长: {(int)data["onlinetime"]}分钟\n" +
                    //$"本期PC在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                    $"本期总在线时长: {(int)data["onlinetime"] / 3600}分钟\n" +
                //$"当前服务器阶段: {((bool)data["online"] ? "肉前阶段" : "肉后阶段" : "巨人前阶段" : "巨人后阶段" : "四柱阶段" : "月后阶段")}");
                //$"状态: {((bool)data["online"] ? $"在线 ({GetCurrentServer(account.username)})" : "离线")}");
                $"状态: {(!string.IsNullOrEmpty(online) ? $"在线 ({online})" : "离线")}");
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
                using var background = 泰拉背包.background.LoadImage();
                using var frame = 泰拉背包.frame.LoadImage();
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
        public class 泰拉玩家
        {
            public static void Main(CommandArgs args)
            {
                args.Callback("当前泰拉服务器玩家列表: \n" + string.Join(" ", Configuration.GetConfig<ServerManager>().GetServer(args)
                    .RunRest("/v2/users/list")["users"].Select((user) => $"[{user["name"]}]")));
            }
        }
        public class 泰拉在线
        {
            private static string GetOnline(string name)
            {
                var arr = new HashSet<string>(subserver.t[name]
                    .SelectMany(name => manager.servers.TryGetValue(name, out var svr) ? svr.GetOnlinePlayers() : Array.Empty<string>()));
                return $"『{name}』在线({arr.Count}/100):\n{string.Join(" ", arr.Select(s => $"[{s}]"))}";
            }

            public static void Main(CommandArgs args)
            {
                args.Callback(string.Join("\n", subserver.t.Select(pair => GetOnline(pair.Key))));
            }
            public static void Main(CommandArgs args, string name)
            {
                args.Callback(GetOnline(name));
            }
        }
    }
}
