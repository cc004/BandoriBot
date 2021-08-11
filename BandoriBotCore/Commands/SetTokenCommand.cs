using BandoriBot.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class SetTokenCommand : ICommand
    {
        public List<string> Alias => new List<string> { "/token" };

        public async Task Run(CommandArgs args)
        {
            var cfg = Configuration.GetConfig<TokenConfig>();
            cfg.t[args.Source.FromQQ] = args.Arg.Trim();
            cfg.Save();
            await args.Callback($"token of {args.Source.FromQQ} set to {args.Arg.Trim()}");
        }
    }
}
