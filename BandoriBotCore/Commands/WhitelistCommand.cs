using BandoriBot.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class WhitelistCommand : HashCommand<Whitelist, long>
    {
        public override List<string> Alias => new List<string>
        {
            "/whitelist"
        };

        protected override long GetTarget(long value) => value;
        protected override string Permission => "management.whitelist";
    }
}
