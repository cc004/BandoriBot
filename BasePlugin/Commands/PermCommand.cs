using BandoriBot.Config;
using BandoriBot.Handler;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class PermCommand : HashListCommand<PermissionConfig, long, string>
    {
        protected override string Permission => "management.perm";
        protected override async Task<bool> HasPermission(Source op, long key, string val)
        {
            if (await op.HasPermission("management.perm", -1)) return true;
            return await op.HasPermission($"management.perm", long.Parse(val.Split('.')[0]));
        }

        public override List<string> Alias => new List<string>
        {
            "/perm"
        };
    }
}
