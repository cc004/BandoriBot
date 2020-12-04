using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

        public bool OnMessage(string message, Source Sender, bool isAdmin, Action<string> callback)
        {
            if (Sender.FromGroup == 0) return false;
            lock (t)
            {
                if (!t.ContainsKey(Sender.FromGroup))
                    t[Sender.FromGroup] = new Dictionary<long, int>();
                Dictionary<long, int> dic = t[Sender.FromGroup];
                if (!dic.ContainsKey(Sender.FromQQ))
                    dic[Sender.FromQQ] = 1;
                else
                    ++dic[Sender.FromQQ];
                Save();
            }
            return false;
        }
    }
}
