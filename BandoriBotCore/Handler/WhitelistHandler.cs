using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    class WhitelistHandler : IMessageHandler
    {
        public bool IgnoreCommandHandled => true;

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            return !Configuration.GetConfig<Whitelist>().hash.Contains(args.Sender.FromGroup) && !args.IsAdmin;
        }
    }
}
