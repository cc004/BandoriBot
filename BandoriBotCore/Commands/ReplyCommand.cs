using BandoriBot.Config;
using BandoriBot.DataStructures;
using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class ReplyCommand : ICommand
    {
        public List<string> Alias => new List<string>
        {
            "/reply"
        };

        public void Run(CommandArgs args)
        {
            string[] splits = args.Arg.Trim().Split(' ');
            if (string.IsNullOrWhiteSpace(args.Arg.Trim()))
            {
                args.Callback("/reply <add/del/list>[1234] ...\n1 is string comparision\n2 is regex match\n3 is regex match without at(requires admin).\n4 is c# code");
                return;
            }

            var config = Configuration.GetConfig<ReplyHandler>();
            var qq = args.Source.FromQQ;

            switch (splits[0])
            {
                case "reload":
                    if (args.IsAdmin)
                    {
                        config.Load();
                        args.Callback("configuration has been reloaded successfully.");
                    }
                    break;
                case "save":
                    if (args.IsAdmin)
                    {
                        config.Save();
                        args.Callback("configuration has been saved successfully.");
                    }
                    break;
                case "add2":
                case "add3":
                case "add4":
                    {
                        if (splits.Length < 3)
                        {
                            args.Callback("Invalid argument count.");
                            return;
                        }

                        try
                        {
                            new Regex($"^{Utils.FixRegex(splits[1])}$");
                        }
                        catch
                        {
                            args.Callback("Invalid regular expression format.");
                            return;
                        }

                        var reply = new Reply
                        {
                            qq = qq,
                            reply = string.Concat(splits.Skip(2).Select((s) => s + ' ')).Trim()
                        };

                        var data = config[int.Parse(splits[0].Substring(3))];

                        if (splits[0] == "add4" && !args.IsAdmin)
                        {
                            args.Callback("Access denied!");
                            return;
                        }

                        var t = data.SingleOrDefault(data => data.Item1.ToString()[1..^1] == splits[1]);

                        if (t != null)
                        {
                            if (t.Item2.Any(r => r.reply == reply.reply))
                            {
                                args.Callback($"`{splits[1]}` => `{reply.reply}` already exists!");
                                return;
                            }
                            t.Item2.Add(reply);
                        }
                        else
                            data.Add(ReplyHandler.D2T(new KeyValuePair<string, List<Reply>>(splits[1], new List<Reply> { reply })));

                        if (splits[0] == "add4")
                        {
                            try
                            {
                                config.ReloadAssembly();
                            }
                            catch (Exception e)
                            {
                                args.Callback(e.ToString());
                                try
                                {
                                    config.Load();
                                }
                                catch
                                {
                                    config.LoadDefault();
                                }
                                break;
                            }
                        }

                        config.Save();
                        args.Callback($"successfully added {(splits[0] == "add" ? "" : "regular expression")}`{splits[1]}` => `{reply.reply}`");

                        break;
                    }
                case "del2":
                case "del3":
                case "del4":
                    {
                        if (splits.Length < 3)
                        {
                            args.Callback("Invalid argument count.");
                            return;
                        }

                        var data = config[int.Parse(splits[0].Substring(3))];
                        var result = Utils.TryGetValueStart(data, (pair) => pair.Item1.ToString()[1..^1], splits[1], out var list);
                        var replystart = string.Concat(splits.Skip(2).Select((s) => s + ' ')).Trim();

                        if (string.IsNullOrEmpty(result))
                        {
                            var result2 = Utils.TryGetValueStart(list.Item2, (reply) => reply.reply, replystart, out var reply);

                            if (string.IsNullOrEmpty(result2))
                            {
                                if (reply.qq == qq || args.IsAdmin)
                                {
                                    list.Item2.Remove(reply);
                                    if (list.Item2.Count == 0)
                                        data.Remove(list);
                                    config.Save();
                                    args.Callback($"successfully removed `{list.Item1.ToString()[1..^1]}` => `{reply.reply}`");
                                }
                                else
                                    args.Callback("Access denied.");
                            }
                            else
                                args.Callback(result2);
                        }
                        else
                            args.Callback(result);

                        break;
                    }
                case "list2":
                case "list3":
                case "list4":
                    {
                        var data = config[int.Parse(splits[0].Substring(4))];
                        if (splits.Length == 1)
                        {
                            if (!args.IsAdmin)
                            {
                                args.Callback("Access denied.");
                                return;
                            }
                            args.Callback("All valid replies:\n" + string.Concat(data.Select((pair) => pair.Item1.ToString() + "\n")));
                        }
                        else if (splits.Length == 2)
                        {
                            var result = Utils.TryGetValueStart(data, (pair) => pair.Item1.ToString()[1..^1], splits[1], out var list);

                            if (string.IsNullOrEmpty(result))
                                args.Callback($"All valid replies for `{list.Item1.ToString()[1..^1]}`:\n{string.Concat(list.Item2.Select((reply) => $"`{reply.reply}` (by {reply.qq})\n"))}");
                            else
                                args.Callback(result);

                        }
                        else
                            args.Callback("Invalid argument count.");
                        break;
                    }
                case "search2":
                case "search3":
                case "search4":
                    {
                        var data = config[int.Parse(splits[0].Substring(6))];

                        if (splits.Length == 1) return;

                        var result = ReplyHandler.FitRegex(data, splits[1]);
                        args.Callback($"All regex that match `{splits[1]}`:\n{string.Join('\n', data.Where(tuple => tuple.Item1.Match(splits[1]).Success).Select(tuple => tuple.Item1.ToString()).Select(str => str[1..^1]))}");
                        break;
                    }
            }
        }
    }
}
