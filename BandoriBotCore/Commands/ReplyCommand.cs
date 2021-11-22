using BandoriBot.Config;
using BandoriBot.DataStructures;
using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task Run(CommandArgs args)
        {
            if (Configuration.GetConfig<Blacklist2>().InBlacklist(args.Source.FromQQ)) return;

            string[] splits = args.Arg.TrimStart().Split(' ');
            if (string.IsNullOrWhiteSpace(args.Arg.Trim()))
            {
                await args.Callback("/reply <add/del/list>[1234] ...\n1 is string comparision\n2 is regex match\n3 is regex match without at(requires admin).\n4 is c# code");
                return;
            }

            var config = Configuration.GetConfig<ReplyHandler>();
            var qq = args.Source.FromQQ;

            switch (splits[0])
            {
                case "reload":
                    if (!await args.Source.HasPermission("reply.reload", -1))
                    {
                        config.Load();
                        await args.Callback("configuration has been reloaded successfully.");
                    }
                    break;
                case "save":
                    if (!await args.Source.HasPermission("reply.save", -1))
                    {
                        config.Save();
                        await args.Callback("configuration has been saved successfully.");
                    }
                    break;
                case "add2":
                case "add3":
                case "add4":
                    {
                        if (splits.Length < 3)
                        {
                            await args.Callback("Invalid argument count.");
                            return;
                        }
                        Regex reg;

                        try
                        {
                            reg = new Regex($"^{Utils.FixRegex(splits[1])}$", RegexOptions.Multiline | RegexOptions.Compiled);
                        }
                        catch
                        {
                            await args.Callback("Invalid regular expression format.");
                            return;
                        }

                        var reply = new Reply
                        {
                            qq = qq,
                            reply = string.Concat(splits.Skip(2).Select((s) => s + ' ')).Trim()
                        };

                        var data = config[int.Parse(splits[0].Substring(3))];

                        if (splits[0] == "add4" && !await args.Source.HasPermission("reply.add4", -1))
                        {
                            await args.Callback("Access denied!");
                            return;
                        }

                        if (data.TryGetValue(splits[1], out var t))
                        {
                            if (t.Any(r => r.reply == reply.reply))
                            {
                                await args.Callback($"`{splits[1]}` => `{reply.reply}` already exists!");
                                return;
                            }
                            t.Add(reply);
                        }
                        else
                        {
                            data.Add(splits[1], new List<Reply> { reply });
                            ReplyHandler.regexCache[splits[1]] = reg;
                        }


                        if (splits[0] == "add4")
                        {
                            try
                            {
                                await config.ReloadAssembly();
                            }
                            catch (Exception e)
                            {
                                await args.Callback(e.ToString());
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
                        await args.Callback($"successfully added {(splits[0] == "add" ? "" : "regular expression")}`{splits[1]}` => `{reply.reply}`");

                        break;
                    }
                case "dela2":
                case "dela3":
                case "dela4":
                {
                    if (splits.Length < 3)
                    {
                        await args.Callback("Invalid argument count.");
                        return;
                    }
                    
                    var data = config[int.Parse(splits[0].Substring(4))];
                    if (!await args.Source.HasPermission("reply.management", -1))
                    {
                        await args.Callback("access denied.");
                        return;
                    }

                    foreach (var pair in data.ToArray())
                    {
                        if (pair.Key.StartsWith(pair.Key)) data.Remove(pair.Key);
                    }

                    config.Save();

                    break;
                }
                case "del2":
                case "del3":
                case "del4":
                    {
                        if (splits.Length < 3)
                        {
                            await args.Callback("Invalid argument count.");
                            return;
                        }

                        var data = config[int.Parse(splits[0].Substring(3))];
                        var result = Utils.TryGetValueStart(data, (pair) => pair.Key, splits[1], out var list);
                        var replystart = string.Concat(splits.Skip(2).Select((s) => s + ' ')).Trim();

                        if (string.IsNullOrEmpty(result))
                        {
                            var result2 = Utils.TryGetValueStart(list.Value, (reply) => reply.reply, replystart, out var reply);

                            if (string.IsNullOrEmpty(result2))
                            {
                                if (reply.qq == qq || await args.Source.HasPermission("reply.deloverride", -1))
                                {
                                    list.Value.Remove(reply);
                                    if (list.Value.Count == 0)
                                        data.Remove(list.Key);
                                    config.Save();
                                    await args.Callback($"successfully removed `{list.Key}` => `{reply.reply}`");
                                }
                                else
                                    await args.Callback("Access denied.");
                            }
                            else
                                await args.Callback(result2);
                        }
                        else
                            await args.Callback(result);

                        break;
                    }
                case "list2":
                case "list3":
                case "list4":
                    {
                        var data = config[int.Parse(splits[0].Substring(4))];
                        if (splits.Length == 1)
                        {
                            if (!await args.Source.HasPermission("reply.list", -1))
                            {
                                await args.Callback("Access denied.");
                                return;
                            }
                            await args.Callback("All valid replies:\n" + string.Concat(data.Select((pair) => pair.Key + "\n")));
                        }
                        else if (splits.Length == 2)
                        {
                            var result = Utils.TryGetValueStart(data, (pair) => pair.Key, splits[1], out var list);

                            if (string.IsNullOrEmpty(result))
                                await args.Callback($"All valid replies for `{list.Key}`:\n{string.Concat(list.Value.Select((reply) => $"`{reply.reply}` (by {reply.qq})\n"))}");
                            else
                                await args.Callback(result);

                        }
                        else
                            await args.Callback("Invalid argument count.");
                        break;
                    }
                case "search2":
                case "search3":
                case "search4":
                    {
                        var data = config[int.Parse(splits[0].Substring(6))];

                        if (splits.Length == 1) return;

                        var result = ReplyHandler.FitRegex(data, string.Join(" ", splits.Skip(1)));
                        await args.Callback($"All regex that match `{splits[1]}`:\n" +
                            $"{string.Join('\n', data.Where(tuple => ReplyHandler.regexCache[tuple.Key].Match(splits[1]).Success).Select(tuple => tuple.Key))}");
                        break;
                    }
            }
        }
    }
}
