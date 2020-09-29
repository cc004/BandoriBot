using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class BlacklistCommand : HashCommand<Blacklist>
    {
        public override List<string> Alias => new List<string>
        {
            "/blacklist"
        };

        public override void Run(CommandArgs args)
        {
            if (!args.IsAdmin) return;
            base.Run(args);
        }
    }
}
