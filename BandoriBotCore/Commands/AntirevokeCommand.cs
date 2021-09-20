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
        protected override long GetTarget(long value) => value;

        public override async Task Run(CommandArgs args)
        {
            if (!await args.Source.CheckPermission()) return;
            await base.Run(args);
        }
    }
    public class AntirevokePlusCommand : HashCommand<AntirevokePlus, long>
    {
        public override List<string> Alias => new List<string> { "/arplus" };
        protected override long GetTarget(long value) => value;

        public override async Task Run(CommandArgs args)
        {
            if (!await args.Source.CheckPermission()) return;
            await base.Run(args);
        }
    }
}
