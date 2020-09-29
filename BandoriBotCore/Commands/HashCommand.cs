using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public abstract class HashCommand<T> : ICommand where T : HashConfiguration
    {
        public abstract List<string> Alias { get; }
        public virtual void Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            var config = Configuration.GetConfig<T>();
            switch (splits[0])
            {
                case "add":
                    config.hash.Add(long.Parse(splits[1]));
                    config.Save();
                    args.Callback($"successfully added {splits[1]}");
                    break;
                case "del":
                    config.hash.Remove(long.Parse(splits[1]));
                    config.Save();
                    args.Callback($"successfully removed {splits[1]}");
                    break;
                case "list":
                    var no = 0;
                    args.Callback(string.Concat(config.hash.Select((g) => $"{++no}. {g}\n")));
                    break;
            }
        }
    }
}
