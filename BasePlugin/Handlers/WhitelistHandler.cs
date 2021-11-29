using BandoriBot.Config;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    class WhitelistHandler : IMessageHandler
    {
        public bool IgnoreCommandHandled => true;

        public float Priority => 1100f;

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            return !Configuration.GetConfig<Whitelist>().hash.Contains(args.Sender.FromGroup) &&
                !await args.Sender.HasPermission("ignore.whitelist", -1);
        }
    }
}
