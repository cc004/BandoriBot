using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public abstract class CooldownCommand<T> : Command where T : Cooldown
    {
        protected abstract TimeSpan DoRun(CommandArgs args);

        protected sealed override void Run(CommandArgs args)
        {
            var config = Configuration.GetConfig<T>();
            if (args.IsAdmin || config.IsExpire(args.Source.FromQQ))
            {
                var span = DoRun(args);
                if (span.Ticks > 0)
                    config.Set(args.Source.FromQQ, span);
            }
            else
            {
                args.Callback("You are in cooldown!");
            }
        }
    }
}
