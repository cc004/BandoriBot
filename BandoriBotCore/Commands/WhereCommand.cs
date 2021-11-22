using BandoriBot.Config;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BandoriBot.Handler;
using BandoriBot.Services;

namespace BandoriBot.Commands
{
    public class WhereCommand : ICommand
    {
        public List<string> Alias => new List<string> { "/where" };
        public async Task Run(CommandArgs args)
        {
            await args.Callback($"{args.Source.FromGroup}{(args.Source.IsGuild ? $"{MessageHandler.GetGroupCache(args.Source.FromGroup)}" : "")} {args.Source.FromQQ}");
        }
    }
}
