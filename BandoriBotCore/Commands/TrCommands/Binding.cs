using System.Collections.Generic;
using System.Linq;
using BandoriBot.Config;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 解绑
        {
            [Superadmin]
            public static void Main(CommandArgs args, string name)
            {
                var lst = Configuration.GetConfig<AccountBinding>().t;
                var lst2 = new List<Binding>();
                foreach (var obj in lst)
                    if ((string)obj.username != name || (int)obj.group != GetServer(args).group)
                        lst2.Add(obj);

                Configuration.GetConfig<AccountBinding>().t = lst2;
                Configuration.GetConfig<AccountBinding>().Save();
                args.Callback(string.Format(GlobalConfiguration.Global.func63Info, name));
            }
        }
        public class 绑定
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string username, long qq, string server)
            {
                if (bindings.t.Any(o => o.username == username || o.qq == qq))
                {
                    args.Callback("你在本客户端已注册了泰拉角色，请输入 泰拉资料 查看你的角色信息");
                    return;
                }

                bindings.t.Add(new Binding
                {
                    username = username,
                    qq = qq,
                    group = manager.servers[server].group
                });

                bindings.Save();

                args.Callback($"你注册的泰拉角色名是{username}=>你的泰拉角色已绑定{qq}可以进入服务器了哦～");
            }
        }
    }
}
