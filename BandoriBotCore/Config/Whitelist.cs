using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class Whitelist : HashConfiguration
    {
        public override string Name => "whitelist.json";
    }
}
