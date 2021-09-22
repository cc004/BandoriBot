using BandoriBot.Config;
using BandoriBot.Terraria;
using System.Linq;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 泰拉财富排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前财富排行如下: ", GetServer(args).RunRest($"/economy/getmoneyrank")["response"],
                    rank => $"共拥有${rank["Value"]}", page, rank => rank.Value<string>("Key") == name, rank => $"[{rank["Key"]}]"));

            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉财富排行 页码");
            }
        }
        public class 泰拉每日在线排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前在线排行如下: ", GetServer(args).RunRest($"/v1/dailyonlinetime/rankboard"),
                    rank => $"共计在线{(int)rank["time"] / 3600}分钟", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉每日在线排行 页码");
            }
        }
        public class 泰拉物品排行
        {
            public static void Main(CommandArgs args, int id, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前物品ID{id}的排行如下: ", GetServer(args).RunRest($"/v1/itemrank/rankboard?&id={id}")
                    .Where(t => (int)t["count"] > 0),
                    rank => $"共拥有{rank["count"]}个", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉物品排行 物品ID 页码");
            }
        }
        public class 泰拉渔夫排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前渔夫任务排行如下: ", GetServer(args).RunRest($"/v1/questrank/rankboard"),
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
                args.Callback(RankFormat($"当前在线排行如下: ", GetServer(args).RunRest($"/v1/onlinetime/rankboard"),
                    rank => $"共计在线{(int)rank["time"] / 3600}分钟", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉在线排行 页码");
            }
        }
        public class 泰拉重生排行
        {
            public static void Main(CommandArgs args, int page)
            {
                var name = GetUsername(args);
                args.Callback(RankFormat($"当前重生次数排行如下: ", GetServer(args).RunRest($"/v1/deathtimes/rankboard"),
                    rank => $"共计重生{rank["times"]}次", page, rank => rank.Value<string>("name") == name, rank => $"[{rank["name"]}]"));
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式:泰拉重生排行 页码");
            }
        }
    }
}
