using BandoriBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class GachaListCommand : PagedListCommand
    {
        protected override List<string> Alias => new List<string>
        {
            "抽卡列表"
        };
        
        public GachaListCommand()
        {
            PiecePerPage = 10;
            int i = 0;
            ItemList = GachaManager.Instance.GetGachas().Select((obj) => $"{++i}. {obj.gachaName}").ToArray();
        }
    }
}
