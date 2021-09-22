using System.Collections.Generic;

namespace BandoriBot.Config
{
    class SubServerMap : SerializableConfiguration<Dictionary<string, List<string>>>
    {
        public override string Name => "subserver.json";
    }
}
