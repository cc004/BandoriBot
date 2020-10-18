using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    class TitleCommand : CooldownCommand<TitleCooldown>
    {
        public override List<string> Alias => new List<string>
        {
            "/title"
        };

        protected override TimeSpan DoRun(CommandArgs args)
        {
            if (args.Source.FromGroup == 0) return new TimeSpan();
            try
            {
                var title = args.Arg.Trim();
                var res = args.Source.Session.SetGroupSpecialTitle(args.Source.FromGroup, args.Source.FromQQ, title, new TimeSpan(-1L));
                args.Callback($"你的头衔已经修改为：`{title}`！");
                return new TimeSpan(1, 0, 0);
            }
            catch
            {

            }
            return new TimeSpan();
        }
    }
}
