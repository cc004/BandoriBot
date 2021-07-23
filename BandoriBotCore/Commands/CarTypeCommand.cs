using BandoriBot.Config;
using BandoriBot.DataStructures;
using BandoriBot.Handler;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class CarTypeCommand : ICommand
    {
        public List<string> Alias => new List<string> { "/cartype" };

        public async Task Run(CommandArgs args)
        {
            var arg = args.Arg.Trim().Split(' ');
            var group = long.Parse(arg[0]);

            if (!await args.Source.HasPermission("management.cartype", group))
            {
                await args.Callback("access denied.");
                return;
            }

            var type = Enum.Parse<CarType>(arg[1]);
            Configuration.GetConfig<CarTypeConfig>()[group] = type;

            await args.Callback($"car type of {group} set to {type}");
        }
    }
}
