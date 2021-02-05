using BandoriBot.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class PermCommand : HashListCommand<PermissionConfig, long, string>, ICommand
    {

        string ICommand.Permission => "management.perm";

        public override List<string> Alias => new List<string>
        {
            "/perm"
        };
    }
}
