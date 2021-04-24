using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class GenshinGacha : ICommand
    {
        public List<string> Alias => new List<string> { "原神抽卡" };

        public async Task Run(CommandArgs args)
        {
            throw new NotImplementedException();
        }
    }
}
