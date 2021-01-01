using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public abstract class HashCommand<T, TValue> : ICommand where T : HashConfiguration<TValue>
    {
        public abstract List<string> Alias { get; }

        public virtual async Task Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            var config = Configuration.GetConfig<T>();
            switch (splits[0])
            {
                case "add":
                    config.hash.Add(splits[1].ParseTo<TValue>());
                    config.Save();
                    await args.Callback($"successfully added {splits[1]}");
                    break;
                case "del":
                    config.hash.Remove(splits[1].ParseTo<TValue>());
                    config.Save();
                    await args.Callback($"successfully removed {splits[1]}");
                    break;
                case "list":
                    var no = 0;
                    await args.Callback(string.Concat(config.hash.Select((g) => $"{++no}. {g}\n")));
                    break;
            }
        }
    }
}
