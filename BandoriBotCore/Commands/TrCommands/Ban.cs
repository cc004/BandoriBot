using System.Linq;
using BandoriBot.Config;
using System;
using BandoriBot.Terraria;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 封ip
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string ip)
            {
                GetServer(args).RunCommand($"/ban addip {ip}");
                args.Callback(string.Format(GlobalConfiguration.Global.func5Info, ip));
            }
        }
        public class 封
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string name)
            {
                try
                {
                    blacklist.hash.Add(Configuration.GetConfig<AccountBinding>().t.Where((o) => o.username == name && o.group == GetServer(args).group).FirstOrDefault().qq);
                    blacklist.Save();
                }
                catch (NullReferenceException)
                {
                    throw new CommandException("该泰拉玩家未在本客户端绑定qq号");
                }
                GetServer(args).RunCommand($"/ban add {name}");
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
                    blacklist.hash.Remove((long)Configuration.GetConfig<AccountBinding>().t
                        .FirstOrDefault((o) => o.username == name && o.group == GetServer(args).group).qq);
                    blacklist.Save();
                }
                catch (NullReferenceException)
                {
                    throw new CommandException("该泰拉玩家未在本客户端绑定qq号");
                }
                GetServer(args).RunCommand($"/ban del {name}");
                args.Callback(string.Format(GlobalConfiguration.Global.func61Info, name));
            }
        }
        public class 解ip
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, string ip)
            {
                GetServer(args).RunCommand($"/ban delip {ip}");
                args.Callback(string.Format(GlobalConfiguration.Global.func61Info, ip));
            }
        }
    }
}
