using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class CarTypeCommand : ICommand
    {
        public List<string> Alias => new List<string> { "/cartype" };

        public async Task Run(CommandArgs args)
        {
            string[] arg = args.Arg.Trim().Split(' ');
            long group = long.Parse(arg[0]);

            if (!await args.Source.HasPermission("management.cartype", group))
            {
                await args.Callback("access denied.");
                return;
            }

            CarType type = Enum.Parse<CarType>(arg[1]);
            Configuration.GetConfig<CarTypeConfig>()[group] = type;

            await args.Callback($"car type of {group} set to {type}");
        }
    }
}
