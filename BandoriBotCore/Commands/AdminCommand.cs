using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class AdminCommand : HashCommand<Admin, long>
    {
        public override List<string> Alias => new List<string>
        {
            "/admin"
        };

        public override async Task Run(CommandArgs args)
        {
            await base.Run(args);
        }
    }
}
