using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BandoriBot.Models
{
    public class RankData
    {
        public long[] timestamp;
        public Dictionary<int, float[]> ranks;

        private static readonly int[] rank = new int[] { 20, 30, 40, 50, 100, 200, 300, 400, 500, 1000, 2000, 3000, 4000, 5000, 10000, 20000, 30000, 40000, 50000, 100000 };

        public static RankData FromFile(string filename)
        {
            var lines = File.ReadAllText(filename).Split('\n').Skip(1).Where(l => !string.IsNullOrEmpty(l));

            var lines2 = lines.Select(l => l.Split(',').Select(s => long.Parse(s)).ToArray()).ToArray();

            long[] last = lines2.Last();
            /*
            lines2 = lines2.Where(l =>
            {
                for (int i = 1; i < l.Length; ++i)
                    if (l[i] != last[i])
                        return true;
                return false;
            }).OrderBy(l => l[0]).ToArray();
            */
            return new RankData
            {
                timestamp = lines2.Select(l => l[0]/* - lines2[0][0]*/).ToArray(),
                ranks = rank.Select((r, i) => (r, lines2.Select(l => (float)l[i + 1]).ToArray())).ToDictionary(t => t.r, t => t.Item2)
            };
        }

        public void ToFile(string filename)
        {
            var sb = new StringBuilder();

            sb.Append($"time,{string.Join(",", ranks.Select(p => $"rank{p.Key}"))}\n");

            sb.Append(string.Join("\n", timestamp.Select((t, i) => $"{t},{string.Join(",", rank.Select(r => ranks[r][i]))}")));

            File.WriteAllText(filename, sb.ToString());
        }
    }

}
