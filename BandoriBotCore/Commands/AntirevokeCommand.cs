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
        protected override string Permission => "management.antirevoke";
    }
    public class AntirevokePlusCommand : HashCommand<AntirevokePlus, long>
    {
        public override List<string> Alias => new List<string> { "/arplus" };
        protected override long GetTarget(long value) => -1;
        protected override string Permission => "management.antirevokeplus";
    }
}
