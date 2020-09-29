using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace BandoriBot.Commands
{
    public class AntirevokeCommand : HashCommand<Antirevoke>
    {
        public override List<string> Alias => new List<string> { "/antirevoke" };
        public override void Run(CommandArgs args)
        {
            if (!args.IsAdmin) return;
            base.Run(args);
        }
    }
}
