using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    class SendCommand : ICommand
    {
        public List<string> Alias => new() { "/send" };
        public async Task Run(CommandArgs args)
        {
            Configuration.GetConfig<Pipe>().SendMsg(args.Source.FromGroup, string.Join('\n', args.Arg.Split(new char[]
            {
                '\r', '\n'
            }, StringSplitOptions.RemoveEmptyEntries)));
        }
    }
}
