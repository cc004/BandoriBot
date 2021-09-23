using BandoriBot.Config;
using BandoriBot.Terraria;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 泰拉注册
        {
            private static readonly HashSet<char> alphabet = new HashSet<char>("1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ");
            public static void Main(CommandArgs args, string username, string password)
            {
                var binding = bindings.t;
                var noreg = GetServer(args)?.noRegister;

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
                if (binding.Any((o) => o.username == username && o.group == GetServer(args).group))
                {
                    args.Callback($"{username} already registered.");
                    return;
                }

                if (binding.Any((o) => o.qq == args.Source.FromQQ && o.group == GetServer(args).group))
                {
                    args.Callback("你在本客户端已注册了泰拉角色，请输入 泰拉资料 查看你的角色信息");
                    return;
                }
                var result = GetServer(args)
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
                    group = GetServer(args).group
                });

                bindings.Save();

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
        public class 重置
        {
            [Superadmin]
            public static void Main(CommandArgs args, string name)
            {
                GetServer(args).RunCommand($"/user del {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func62Info, name));
            }
        }
        public class 执行
        {
            [Superadmin]
            public static void Default(CommandArgs args)
            {
                if (args.Arg == null || args.Arg == "")
                    args.Callback("您未定义指令，请输入正确指令！");
                else
                    args.Callback(string.Join("\n", GetServer(args)
                        .RunCommand(Utils.FixRegex(args.Arg))["response"].Select(s => s.ToString())));
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

                manager.SwitchTo(args.Source.FromQQ, name);

                if (bindings.t.Any(o => o.qq == args.Source.FromQQ && o.group == manager.servers.FirstOrDefault(s => s.Key == name).Value.group))
                {
                    args.Callback($"你的客户端和角色已切换到{name}，可以进入泰拉服务器了");
                }
                else
                {
                    args.Callback($"你的客户端已切换到{name}，请在本客户端注册你的泰拉角色才能进入服务器哦～请私聊我输入:泰拉注册");
                }
            }
        }
        public class 服务器列表
        {
            public static void Main(CommandArgs args)
            {
                //args.Callback(string.Join("\n", Configuration.GetConfig<ServerManager>().servers.Where(pair => pair.Value.display).Select(pair => pair.Key)));
                args.Callback(string.Join("\n", manager.servers.Select(pair => pair.Key)));
            }
        }
    }
}
