using Newtonsoft.Json.Linq;

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
    public class DailyConfig : HashConfiguration<long>
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
