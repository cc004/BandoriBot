using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class ScoreConfig : DictConfiguration<long, int>
    {
        public override string Name => "scores.json";
    }
    public class LZConfig : DictConfiguration<long, string>
    {
        public override string Name => "lz.json";
    }
    public class DailyConfig : HashConfiguration
    {
        public override string Name => "daily.json";
    }
    public class RandomList : JsonConfiguration
    {
        public override string Name => "random.json";

        public override void LoadDefault()
        {
            json = new JArray();
        }
    }
}
