using BandoriBot.Config;
using Mirai_CSharp.Models;
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

        protected virtual GroupPermission AddPermission => GroupPermission.Administrator;
        protected virtual GroupPermission DelPermission => GroupPermission.Administrator;
        protected virtual long GetTarget(TValue value) => 0;

        public virtual async Task Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            var config = Configuration.GetConfig<T>();
            TValue val;

            switch (splits[0])
            {
                case "add":
                    val = splits[1].ParseTo<TValue>();
                    if (!await args.Source.CheckPermission(GetTarget(val)))
                    {
                        await args.Callback("access denied.");
                        break;
                    }

                    config.hash.Add(val);
                    config.Save();
                    await args.Callback($"successfully added {splits[1]}");
                    break;
                case "del":
                    val = splits[1].ParseTo<TValue>();
                    if (!await args.Source.CheckPermission(GetTarget(val)))
                    {
                        await args.Callback("access denied.");
                        break;
                    }

                    config.hash.Remove(val);
                    config.Save();
                    await args.Callback($"successfully removed {splits[1]}");
                    break;
                case "list":
                    if (!await args.Source.CheckPermission())
                    {
                        await args.Callback("access denied.");
                        break;
                    }
                    var no = 0;
                    await args.Callback(string.Concat(config.hash.Select((g) => $"{++no}. {g}\n")));
                    break;
            }
        }
    }
}
