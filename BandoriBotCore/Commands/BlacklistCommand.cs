using BandoriBot.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class BlacklistCommand : HashCommand<BlacklistF, string>
    {
        public override List<string> Alias => new()
        {
            "/blacklist"
        };

        protected override long GetTarget(string value)
        {
            try
            {
                return long.Parse(value.Split('.')[0]);
            }
            catch
            {
                return -1;
            }
        }

        protected override string Permission => "management.blacklist";
    }
}
