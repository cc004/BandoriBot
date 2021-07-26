using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class TokenConfig : SerializableConfiguration<Dictionary<long, string>>
    {
        public override string Name => "token.json";
    }
}
