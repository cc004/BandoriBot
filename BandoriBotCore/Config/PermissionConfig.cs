using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class PermissionConfig : SerializableConfiguration<Dictionary<long, HashSet<string>>>
    {
        public override string Name => "perms.json";
    }
}
