using BandoriBot.Services;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class GachaCommand : ICommand
    {
        public List<string> Alias => new List<string>
        {
            "抽卡模拟",
        };

        public async Task Run(CommandArgs args)
        {
            var Gachas = GachaManager.Instance.GetGachas();
            int max = Gachas.Length;
            int index = 1;
            if (!string.IsNullOrWhiteSpace(args.Arg) && int.TryParse(args.Arg, out var res))
                index = res;
            index = index < 1 ? 1 : index > max ? max : index;

            var tuple = await GachaManager.Instance.Gacha(Gachas[index - 1].gachaId);
            string code;

            using (var scaled = new Bitmap(tuple.Item2, new Size(tuple.Item2.Width / 4, tuple.Item2.Height / 4)))
                code = Utils.GetImageCode(scaled);
            tuple.Item2.Dispose();
            await args.Callback($"[mirai:at={args.Source.FromQQ}]的{tuple.Item1}：\n{code}");

        }
    }
}
