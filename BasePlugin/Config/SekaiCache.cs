using System.Collections.Generic;

namespace BandoriBot.Config
{
    public class SekaiCache : SerializableConfiguration<Dictionary<long, long>>
    {
        public override string Name => "sekaicache.json";
    }
}
