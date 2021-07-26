using BandoriBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class RecordCommand : ICommand
    {
        public RecordCommand()
        {
        }

        public List<string> Alias => new List<string> { "有多少人在说" };

        public async Task Run(CommandArgs args)
        {
            if (!await args.Source.HasPermission("chatrecord", -1)) return;
            await args.Callback(RecordDatabaseManager.CountContains(args.Arg).ToString());
        }
    }
}
