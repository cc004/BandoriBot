using BandoriBot.Config;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public abstract class Activator : ICommand
    {
        private static string GetTrans(bool status)
        {
            return status ? "开启" : "关闭";
        }
        public abstract List<string> Alias { get; }
        protected abstract bool Status { get; }
        public async Task Run(CommandArgs args)
        {
            args.Arg = args.Arg.Trim();
            if (args.Arg.Length == 0)
            {
                Configuration.GetConfig<Activation>()[args.Source.FromQQ] = Status;
                await args.Callback($"个人车牌转发已{GetTrans(Status)}");
            }
        }

    }
    public class Activate : Activator
    {
        protected override bool Status => true;

        public override List<string> Alias => new List<string>
        {
            "/activate"
        };
    }
    public class Deactivate : Activator
    {
        protected override bool Status => false;

        public override List<string> Alias => new List<string>
        {
            "/deactivate"
        };
    }
}
