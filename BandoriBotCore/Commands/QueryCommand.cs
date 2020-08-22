using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BandoriBot.Config;
using BandoriBot.Handler;

namespace BandoriBot.Commands
{
    public class QueryCommand : Command
    {
        protected override List<string> Alias => new List<string>
        {
            "/query"
        };

        protected override void Run(CommandArgs args)
        {
            MessageStatistic data = Configuration.GetConfig<MessageStatistic>();

            if (args.IsAdmin)
            {
                string[] splits = args.Arg.Trim().Split(' ');
                if (splits.Length > 0)
                { 
                    switch (splits[0])
                    {
                        case "reset":
                            lock (data.Data)
                                data.Data = new Dictionary<long, Dictionary<long, int>>();
                            data.Save();
                            args.Callback("Record has been reset successfully.");
                            return;
                        case "set":
                            long gr = splits[1] == "~" ? args.Source.FromGroup : long.Parse(splits[1]);
                            long qq = splits[1] == "~" ? args.Source.FromQQ : long.Parse(splits[2]);
                            int val = int.Parse(splits[3]);
                            if (gr > 0 && qq > 0)
                            {
                                lock (data.Data)
                                    data.Data[gr][qq] = val;
                                data.Save();
                                args.Callback($"data changed [{gr},{qq}] => {val}.");
                            }
                            else
                            {
                                args.Callback("Invalid argument.");
                            }
                            return;
                    }
                }
            }
            string arg = args.Arg.Trim();
            long group = 0L;
            if (string.IsNullOrEmpty(arg))
                group = args.Source.FromGroup;
            else
                long.TryParse(arg, out group);
            if (group == 0L)
            {
                args.Callback("Invalid group number specified");
                return;
            }
            List<Tuple<long, int>> sort;

            lock (data.Data)
            {
                if (!data.Data.ContainsKey(group))
                {
                    args.Callback($"No record from the group {group} till now.");
                    return;
                }

                sort = new List<Tuple<long, int>>();

                foreach (KeyValuePair<long, int> num in data.Data[group])
                    sort.Add(new Tuple<long, int> (num.Key, num.Value));
            }

            sort.Sort(delegate (Tuple<long, int> var1, Tuple<long, int> var2)
            {
                return var2.Item2 - var1.Item2;
            });

            string result = "";

            try
            {
                for (int i = 0; i < 10; ++i)
                {
                    result += $"{i + 1}.{args.Source.Session.GetName(group, sort[i].Item1)} {sort[i].Item2}pcs\n";
                }
            }
            catch (ArgumentOutOfRangeException) { }

            args.Callback(result);
        }
    }
}