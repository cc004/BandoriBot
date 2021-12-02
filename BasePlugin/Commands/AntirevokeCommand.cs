using BandoriBot.Config;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BandoriBot.Services;

namespace BandoriBot.Commands
{
    public class AntirevokePlusPlusCommand : ICommand
    {
        public List<string> Alias => new List<string> { "/ar" };
        public async Task Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            var group = long.Parse(splits[0]);
            var qq = long.Parse(splits[1]);
            var id = int.Parse(splits[2]);
            if (!await args.Source.HasPermission("management.antirevoke", group) && qq != args.Source.FromQQ) return;
            await args.Callback(RecordDatabaseManager.GetRecords().Where(r => r.qq == qq && r.group == group).SkipLast(id).LastOrDefault()?.message ?? "");
        }
    }
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
