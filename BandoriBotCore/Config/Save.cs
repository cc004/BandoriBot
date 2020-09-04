using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BandoriBot.Config
{
    public class Save : DictConfiguration<long, JObject>
    {
        public override string Name => "save.json";
    }
}
