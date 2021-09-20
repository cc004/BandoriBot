using System.Collections.Generic;

namespace BandoriBot.Config
{
    public class TokenConfig : SerializableConfiguration<Dictionary<long, string>>
    {
        public override string Name => "token.json";
    }
}
