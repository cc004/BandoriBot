using BandoriBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class ZMCCommand : ICommand
    {
        public List<string> Alias => new List<string> { "怎么拆" };

        public async Task Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            Team[] teams;

            if (splits.Length != 5)
            {
                await args.Callback("必须有且只有五个角色！");
                return;
            }

            try
            {
                teams = await JJCManager.Instance.Callapi(splits);
            }
            catch (Exception e)
            {
                await args.Callback(e.Message);
                return;
            }

            var img = JJCManager.Instance.GetImage(teams.Take(10).ToArray()).Resize(0.5f);
            await args.Callback($"{args.Arg.Trim()}的解法：\n" + Utils.GetImageCode(img));
        }
    }
}
