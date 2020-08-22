using BandoriBot.Services;
using PCRClientTest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class PeriodRank : Configuration
    {
        public override string Name => "periodrank.csv";
        private string filebuf = "";
        private DateTime lastSync;
        private static readonly DateTime standard = new DateTime(2020, 7, 1, 5, 0, 0);

        private void Start()
        {
            ScheduleManager.QueueTimed(() =>
            {
                var now = DateTime.Now;
                int span1 = (int)(now - standard).TotalMinutes, span2 = (int)(lastSync - standard).TotalMinutes;

                if (span1 / 30 != span2 / 30 && span1 % 30 > 5)
                {
                    lastSync = now;
                    filebuf += $"{(standard + new TimeSpan(0, span1 / 30 * 30, 0)).ToString()}," +
                        string.Join(",", Enumerable.Range(1, 28).Select(i => PCRManager.Instance.GetRankDamage(i * 100))) +
                        "\n";
                    Save();
                }
            }, 60 * 5);
        }

        public override void LoadDefault()
        {
            var sb = new StringBuilder("time");
            for (int i = 0; i < 28; ++i)
                sb.Append($",{100 * (i + 1)}");
            sb.Append('\n');
            filebuf = sb.ToString();
            Start();
        }

        public override void LoadFrom(BinaryReader br)
        {
            using (var sr = new StreamReader(br.BaseStream))
                filebuf = sr.ReadToEnd();
            var time = filebuf.Split('\n').Last().Split(',').First();
            if (DateTime.TryParse(time, out var value))
                lastSync = value;
            Start();
        }

        public override void SaveTo(BinaryWriter bw)
        {
            using (var sw = new StreamWriter(bw.BaseStream))
                sw.Write(filebuf);
        }
    }
}
