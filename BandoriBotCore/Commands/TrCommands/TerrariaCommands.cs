using System.Linq;
using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BandoriBot.Terraria;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        private static readonly ServerManager manager = Configuration.GetConfig<ServerManager>();
        private static readonly AccountBinding bindings = Configuration.GetConfig<AccountBinding>();
        private static readonly LZConfig lz = Configuration.GetConfig<LZConfig>();
        private static readonly DailyConfig daily = Configuration.GetConfig<DailyConfig>();
        private static readonly ScoreConfig score = Configuration.GetConfig<ScoreConfig>();
        private static readonly GlobalConfiguration global = Configuration.GetConfig<GlobalConfiguration>();
        private static readonly Blacklist blacklist = Configuration.GetConfig<Blacklist>();
        private static readonly SubServerMap subserver = Configuration.GetConfig<SubServerMap>();

        private static Server GetServer(CommandArgs args)
        {
            return manager.GetServer(args);
        }

        private static string GetUsername(CommandArgs args) =>
            bindings.t.Where(o => o.qq == args.Source.FromQQ && o.group == GetServer(args).group).FirstOrDefault()?.username?.ToString();

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
    }
}
