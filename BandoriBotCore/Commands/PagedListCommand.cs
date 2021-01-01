using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public abstract class PagedListCommand : ICommand
    {
        protected string[] ItemList;
        protected int PiecePerPage;

        public abstract List<string> Alias { get; }
        public async Task Run(CommandArgs args)
        {
            int page = 1;
            int maxPage = (int)Math.Floor((float)ItemList.Length / PiecePerPage);
            if (!string.IsNullOrWhiteSpace(args.Arg) && int.TryParse(args.Arg, out var res))
                page = res;
            page = page < 1 ? 1 : page > maxPage ? maxPage : page;

            await args.Callback(string.Join("\n", ItemList
                .Skip((page - 1) * PiecePerPage)
                .Take(Math.Min(PiecePerPage, ItemList.Length - PiecePerPage * (page - 1)))) +
                $"\n({page}/{maxPage}) 命令后页码可以翻页");
        }
    }
}
