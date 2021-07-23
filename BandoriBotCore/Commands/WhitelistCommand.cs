using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public override async Task Run(CommandArgs args)
        {
            await base.Run(args);
        }
    }
}
