using System;
using BandoriBot.Handler;

namespace BandoriBot.Config
{
    public class Blacklist : HashConfiguration<long>
    {
        public override string Name => "blacklist.json";
    }
}
