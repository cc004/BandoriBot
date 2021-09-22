using BandoriBot.Config;
using BandoriBot.Handler;
using System;
using System.Linq;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 查黑
        {
            public static void Main(CommandArgs args, long QQ)
            {
                int i = 0;
                var list = string.Join("\n", blacklist.hash.Select(qq => $"{++i}. {qq}"));
                args.Callback((list.Contains(QQ.ToString()) ? "该用户在黑名单内" : "该用户不在黑名单内"));
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
                //TODO: replace with rank format
                //args.Callback("黑名单列表如下\n" + string.Join("\n", Configuration.GetConfig<Blacklist>().hash.Select(qq => $"{++i}. {qq}")));
                var list = blacklist.hash.ToArray();
                for (int i = Math.Min((page - 1), list.Length / 20) * 20; i < Math.Min(page * 20, list.Length); i++)
                {
                    info += $"\n{i + 1}. {list[i]}";
                }
                info += $"\n第[{Math.Min(list.Length / 20 + 1, page)}/{list.Length / 20 + 1}]页";
                args.Callback(info);
            }
        }
        public class 黑名单
        {
            public static void Main(CommandArgs args)
            {
                args.Callback("命令列表：\n添加黑名单 QQ\n移除黑名单\n黑名单列表 页码\n查黑 QQ");
            }
        }
        public class 加入黑名单
        {
            [Permission("terraria.admin")]
            public static void Main(CommandArgs args, long qq)
            {
                if (!Source.AdminQQs.Contains(qq))
                {
                    blacklist.hash.Add(qq);
                    blacklist.Save();
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
                blacklist.hash.Remove(qq);
                blacklist.Save();
                args.Callback(qq + "已移除黑名单");
            }
        }
    }
}
