using BandoriBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class KeywordCommand : ICommand
    {
        public List<string> Alias => new List<string> { "/keyword" };

        public async Task Run(CommandArgs args)
        {
            if (!await args.Source.HasPermission("keyword", -1)) return;

            var a = args.Arg.Split(' ');
            var rec = KeywordRecordContext.context.Records;
            switch (a[0])
            {
                case "add":
                    if (!rec.Any(r => r.Keyword == a[1])) rec.Add(new KeywordRecord { Keyword = a[1], Count =
                        ChatRecordContext.Context.Records.Where(r => r.Message.Contains(a[1])).Count()
                    });
                    await args.Callback($"added {a[1]}");
                    break;
                case "del":
                    var r = rec.FirstOrDefault(r => r.Keyword == a[1]);
                    if (r != null)
                        rec.Remove(r);
                    await args.Callback($"removed {a[1]}");
                    break;
                case "search":
                    if (a.Length == 1)
                        await args.Callback(string.Join('\n', rec.Select(k => $"{k.Keyword}: {k.Count}")));
                    else
                        await args.Callback(string.Join('\n', a.Skip(1).Select(s => rec.First(r => r.Keyword == s)).Select(k => $"{k.Keyword}: {k.Count}")));
                    break;
            }
        }
    }
}
