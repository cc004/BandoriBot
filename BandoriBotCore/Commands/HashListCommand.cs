using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BandoriBot.Config;
using BandoriBot.Handler;
using Mirai_CSharp.Models;

namespace BandoriBot.Commands
{
    public abstract class HashListCommand<T, TKey, TValue> : ICommand where T : SerializableConfiguration<Dictionary<TKey, HashSet<TValue>>>
    {
        public abstract List<string> Alias { get; }

        protected abstract string Permission { get; }
        protected virtual async Task<bool> HasPermission(Source op, TKey key, TValue val) => await op.HasPermission(Permission, op.FromGroup);

        public virtual async Task Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            var config = Configuration.GetConfig<T>();
            var key = splits[1].ParseTo<TKey>();

            switch (splits[0])
            {
                case "add":
                case "del":
                    var val = splits[2].ParseTo<TValue>();
                    var isadd = splits[0] == "add";

                    if (!await HasPermission(args.Source, key, val))
                    {
                        await args.Callback("access denied.");
                        break;
                    }

                    if (isadd)
                    {
                        if (!config.t.ContainsKey(key)) config.t[key] = new HashSet<TValue>();
                        config.t[key].Add(val);
                        config.Save();
                        await args.Callback($"successfully added {splits[1]} => {splits[2]}");
                    }
                    else
                    {
                        config.t[key].Remove(val);
                        if (config.t[key].Count == 0) config.t.Remove(key);
                        config.Save();
                        await args.Callback($"successfully removed {splits[1]} => {splits[2]}");
                    }
                    break;
                case "list":
                    if (!await args.Source.HasPermission(Permission, -1))
                    {
                        await args.Callback("access denied.");
                        break;
                    }
                    var no = 0;
                    await args.Callback(string.Concat(config.t[key].Select((g) => $"{++no}. {g}\n")));
                    break;
            }
        }
    }
}
