using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public abstract class CooldownCommand<T> : ICommand where T : Cooldown
    {
        public abstract List<string> Alias { get; }

        protected abstract TimeSpan DoRun(CommandArgs args);

        public async Task Run(CommandArgs args)
        {
            var config = Configuration.GetConfig<T>();
            if (await args.Source.CheckPermission() || config.IsExpire(args.Source.FromQQ))
            {
                var span = DoRun(args);
                if (span.Ticks > 0)
                    config.Set(args.Source.FromQQ, span);
            }
            else
            {
                await args.Callback("You are in cooldown!");
            }
        }
    }
}
