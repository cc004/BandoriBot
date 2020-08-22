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
        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            return !Configuration.GetConfig<Whitelist>().hash.Contains(Sender.FromGroup) && !isAdmin;
        }
    }
}
