using BandoriBot.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public class MessageStatistic : SerializableConfiguration<Dictionary<long, Dictionary<long, int>>>, IMessageHandler
    {
        public override string Name => "statistic.json";

        public bool IgnoreCommandHandled => true;

        public override void LoadDefault()
        {
            t = new Dictionary<long, Dictionary<long, int>>();
        }

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            if (args.Sender.FromGroup == 0) return false;

            if (!t.ContainsKey(args.Sender.FromGroup))
                t[args.Sender.FromGroup] = new Dictionary<long, int>();
            Dictionary<long, int> dic = t[args.Sender.FromGroup];
            if (!dic.ContainsKey(args.Sender.FromQQ))
                dic[args.Sender.FromQQ] = 1;
            else
                ++dic[args.Sender.FromQQ];
            return false;
        }
    }
}
