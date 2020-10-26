using BandoriBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class Blacklist : HashConfiguration<long>
    {
        public override string Name => "blacklist.json";
    }

    public class BlacklistF : HashConfiguration<string>
    {
        public override string Name => "blacklistf.json";

        public bool InBlacklist(long group, object function)
        {
            return hash.Contains($"{group}.{function.GetType().Name}");
        }
    }
}
