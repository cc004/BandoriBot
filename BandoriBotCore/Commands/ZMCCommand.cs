using BandoriBot.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class ZMCCommand : ICommand
    {
        public List<string> Alias => new List<string> { "怎么拆" };

        public async Task Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            string code;

            code = await JJCManager.Instance.Callapi(args.Arg.Trim());
            await args.Callback($"{args.Arg.Trim()}的解法：\n" + code);
        }
    }
}
