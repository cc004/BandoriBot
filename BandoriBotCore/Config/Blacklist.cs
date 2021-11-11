using System;
using BandoriBot.Handler;

namespace BandoriBot.Config
{
    public class Blacklist : HashConfiguration<long>
    {
        public override string Name => "blacklist.json";
    }

    public class BlacklistF : HashConfiguration<string>
    {
        public override string Name => "blacklistf.json";

        public bool InBlacklist(long group, object function)
        {
            if (function is HandlerHolder holder) function = holder.handler;
            return hash.Contains($"{group}.{function.GetType().Name}");
        }
    }
    public class Blacklist2 : HashConfiguration<string>
    {
        public override string Name => "blacklist2.json";

        public bool InBlacklist(long qq)
        {
            return hash.Contains(qq.ToString());
        }
    }
}
