using BandoriBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    [JsonObject]
    public class Boss
    {
        public string name;
        public int value;
        public float[] multiplier;
    }

    public class PCRConfig : Configuration
    {
        [JsonObject]
        private class Record
        {
            public long qq;
            public int damage, nowboss, nowturn;
            public bool extra;
            public DateTime time;
        }

        [JsonObject]
        private class JsonConfig
        {
            public List<Record> records = new List<Record>();
            public List<long> extras = new List<long>();
            public int nowturn = 0;
            public int nowboss = 0;
            public int remaining = 0;
        }

        public override string Name => "pcr.json";
        private JsonConfig data;
        public List<Boss> bossInfo;

        public void SetData(int turn, int boss, int remaining)
        {
            data.nowturn = turn;
            data.nowboss = boss;
            data.remaining = remaining;
        }

        private void Update()
        {
            if (data.nowturn == 0)
            {
                data.nowturn = 1;
                data.nowboss = 0;
                data.remaining = bossInfo[0].value;
            }
            else
            {
                if (data.remaining == 0)
                {
                    ++data.nowboss;
                    if (data.nowboss == bossInfo.Count)
                    {
                        data.nowboss = 0;
                        data.nowturn++;
                    }
                    data.remaining = bossInfo[data.nowboss].value;
                }
            }
        }

        public override void LoadDefault()
        {
            data = new JsonConfig();
            bossInfo = JsonConvert.DeserializeObject<List<Boss>>(File.ReadAllText("bossinfo.json"));
            Update();
        }

        public override void LoadFrom(BinaryReader br)
        {
            data = JsonConvert.DeserializeObject<JsonConfig>(new StreamReader(br.BaseStream).ReadToEnd());
            bossInfo = JsonConvert.DeserializeObject<List<Boss>>(File.ReadAllText("bossinfo.json"));
            Update();
        }

        public override void SaveTo(BinaryWriter bw)
        {
            var sw = new StreamWriter(bw.BaseStream);
            sw.Write(JsonConvert.SerializeObject(data));
            sw.Flush();
        }

        private void DealDamage(long qq, int damage, bool isextra)
        {
            data.records.Add(new Record
            {
                qq = qq,
                damage = damage,
                nowboss = data.nowboss,
                nowturn = data.nowturn,
                extra = isextra,
                time = DateTime.Now
            });

            data.remaining -= damage;
            Update();
            Save();
        }

        public List<long> Query(DateTime start, DateTime end)
        {
            Dictionary<long, int> times = new Dictionary<long, int>();
            foreach (var record in data.records)
            {
                if (record.damage == 0) continue;
                if (record.time < start || record.time > end) continue;
                if (!times.ContainsKey(record.qq))
                {
                    times.Add(record.qq, 0);
                }

                ++times[record.qq];
            }

            return times.Where(pair => pair.Value < 3).Select(pair => pair.Key).ToList();
        }

        public string Query(DateTime start, DateTime end, Func<long, string> namegetter)
        {
            Dictionary<long, int> damage = new Dictionary<long, int>();
            Dictionary<long, string> info = new Dictionary<long, string>();
            foreach (var record in data.records)
            {
                if (record.damage == 0) continue;
                if (record.time < start || record.time > end) continue;
                if (!damage.ContainsKey(record.qq))
                {
                    damage.Add(record.qq, 0);
                    info.Add(record.qq, "");
                }

                damage[record.qq] += record.damage;
                info[record.qq] += record.extra ? $"({record.nowboss + 1})" : (record.nowboss + 1).ToString();
            }

            return string.Join("\n", damage.OrderByDescending(pair => pair.Value).Select(pair => $"{namegetter(pair.Key)} {pair.Value} {info[pair.Key]}"));
        }

        public string GetCSV(DateTime start, DateTime end, Func<long, string> namegetter)
        {
            var sb = new StringBuilder();
            var now = DateTime.Now;
            var records = from r in data.records where r.time > start && r.time < end && r.damage > 1000 && r.damage < 2000000 select r;
            var qqs = new HashSet<long>();
            var boss = new HashSet<int>();
            var avgd = new Dictionary<int, float>();

            foreach (var record in records)
            {
                boss.Add(record.nowturn * 10 + record.nowboss);
                qqs.Add(record.qq);
            }

            foreach (var b in boss)
            {
                int s = 0, n = 0;
                foreach (var record in from r in records where !r.extra && r.nowboss + r.nowturn * 10 == b select r)
                {
                    if (record.damage == 0) continue;
                    ++n;
                    s += record.damage;
                }
                avgd.Add(b, n > 0 ? s / n : float.MaxValue);
            }

            sb.AppendLine($"从{start}到{end}的出刀记录");
            sb.AppendLine($"QQ,昵称,总伤害,平均出刀质量,平均每刀质量");
            
            foreach (var qq in qqs)
            {
                var sb2 = new StringBuilder();
                var quality = 0f;
                var damage = 0;
                var n = 0;

                foreach (var record in from r in records where r.qq == qq && r.damage > 0 select r)
                {
                    if (!record.extra) ++n;
                    quality += record.damage / avgd[record.nowboss + record.nowturn * 10];
                    damage += record.damage;
                    sb2.Append($",{record.damage},{(char)('A' + record.nowturn - 1)}{record.nowboss + 1}{(record.extra ? "尾刀" : "")}");
                }

                sb.AppendLine($"{qq},{namegetter(qq)},{damage},{(int)(quality * 1000) / 10f}%,{(int)(quality / n * 1000) / 10f}%{sb2.ToString()}");
            }

            return sb.ToString();
        }

        public string Add(long qq, int damage)
        {
            string result = "";
            if (damage > data.remaining)
                throw new Exception("出刀伤害大于剩余血量！");

            if (damage > 0)
            {
                bool extra = data.extras.Contains(qq);
                if (extra) data.extras.Remove(qq);
                result += $"{{0}}{(extra ? "尾刀" : "")}对{bossInfo[data.nowboss].name}造成了{damage}伤害{(data.remaining > damage ? "" : "并击破")}\n";

                if (data.remaining == damage && !extra) data.extras.Add(qq);
                DealDamage(qq, damage, extra);
            }

            return result + $"现在是{data.nowturn}周目, Boss{data.nowboss + 1}({bossInfo[data.nowboss].name})，剩余血量{data.remaining}";
        }
    }

}
