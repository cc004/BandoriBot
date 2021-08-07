using System;
using System.Collections.Generic;
using System.Text;

namespace BandoriBot.Config
{
    public class SekaiCache : SerializableConfiguration<Dictionary<long, long>>
    {
        public override string Name => "sekaicache.json";
    }
}
