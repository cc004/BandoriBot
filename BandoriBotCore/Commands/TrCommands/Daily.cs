using BandoriBot.Config;
using Newtonsoft.Json.Linq;
using System;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        private static Random rnd = new();

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
                if (daily.hash.Contains(args.Source.FromQQ)) return;
                var a = new Random().Next(7, 15);
                args.Callback($"玩家{args.Source.GetName()}获得{a}积分随机，积分每日重置请尽量用完哦（用于抽奖，兑换建筑材料）");
                daily.hash.Add(args.Source.FromQQ);
                score[args.Source.FromQQ] += a;
                Configuration.GetConfig<ScoreConfig>().Save();
            }
        }
        public class 兑换龙珠
        {
            const string list = "①②③④⑤⑥⑦⑨⑩";
            public static void Main(CommandArgs args)
            {
                var s = score[args.Source.FromQQ];
                if (s < 5) return;
                score[args.Source.FromQQ] -= 5;
                var a = list[rnd.Next(list.Length)];
                args.Callback($"玩家[{Utils.GetName(args.Source)}]获得龙珠{a}消耗了5积分");
                lz[args.Source.FromQQ] += a;
            }
        }
        public class 龙珠背包
        {
            public static void Main(CommandArgs args)
            {
                args.Callback($"玩家[{Utils.GetName(args.Source)}]的龙珠背包有{lz[args.Source.FromQQ]}");
            }
        }
    }
}
