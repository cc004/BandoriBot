using System.Collections.Generic;

namespace BandoriBot.Config
{
    public class PermissionConfig : SerializableConfiguration<Dictionary<long, HashSet<string>>>
    {
        public override string Name => "perms.json";
    }
}
