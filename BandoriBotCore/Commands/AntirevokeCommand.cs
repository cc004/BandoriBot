using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class AntirevokeCommand : HashCommand<Antirevoke, long>
    {
        public override List<string> Alias => new List<string> { "/antirevoke" };
        public override async Task Run(CommandArgs args)
        {
            if (!args.IsAdmin) return;
            await base.Run(args);
        }
    }
}
