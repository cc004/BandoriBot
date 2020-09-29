using BandoriBot.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace BandoriBot.Commands
{
    public class GachaCommand : ICommand
    {
        public List<string> Alias => new List<string>
        {
            "抽卡模拟",
        };

        public void Run(CommandArgs args)
        {
            var Gachas = GachaManager.Instance.GetGachas();
            int max = Gachas.Length;
            int index = 1;
            if (!string.IsNullOrWhiteSpace(args.Arg) && int.TryParse(args.Arg, out var res))
                index = res;
            index = index < 1 ? 1 : index > max ? max : index;

            var tuple = GachaManager.Instance.Gacha(Gachas[index - 1].gachaId);
            string code;

            using (var scaled = new Bitmap(tuple.Item2, new Size(tuple.Item2.Width / 4, tuple.Item2.Height / 4)))
                code = Utils.GetImageCode(scaled);
            tuple.Item2.Dispose();
            args.Callback($"[mirai:at={args.Source.FromQQ}]的{tuple.Item1}：\n{code}");

        }
    }
}
