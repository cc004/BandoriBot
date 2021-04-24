using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BandoriBot.Config;

namespace BandoriBot.Commands
{
    class SendCommand : ICommand
    {
        public List<string> Alias => new (){"/send"};
        public async Task Run(CommandArgs args)
        {
            Configuration.GetConfig<Pipe>().SendMsg(args.Source.FromGroup, string.Join('\n', args.Arg.Split(new char[]
            {
                '\r', '\n'
            }, StringSplitOptions.RemoveEmptyEntries)));
        }
    }
}
