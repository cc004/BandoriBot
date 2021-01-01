using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class DelayCommand : ICommand
    {
        public List<string> Alias => new List<string>
        {
            "/delay"
        };
        public async Task Run(CommandArgs args)
        {
            try
            {
                int expected = int.Parse(args.Arg.Trim());
                expected = Math.Max(0, Math.Min(60, expected));
                Configuration.GetConfig<Config.Delay>()[args.Source.FromQQ] = expected;
                await args.Callback($"车牌转发延迟已更改为{expected}秒");
            }
            catch { }
        }
    }
}
