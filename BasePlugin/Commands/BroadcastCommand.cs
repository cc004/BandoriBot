using BandoriBot.Config;
using System.Collections.Generic;
using System.Threading.Tasks;
using BandoriBot.Handler;

namespace BandoriBot.Commands
{
    public class BroadcastCommand : ICommand
    {
        public List<string> Alias => new()
        {
            "/broadcast"
        };
        public async Task Run(CommandArgs args)
        {
            if (!await args.Source.HasPermission("management.broadcast", -1))
            {
                await args.Callback("access denied.");
            }

            await MessageHandler.Broadcast(args.Arg);
        }
    }
}
