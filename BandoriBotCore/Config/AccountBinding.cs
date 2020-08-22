using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    class AccountBinding : JConfiguration<JObject>
    {
        public override string Name => "account.json";
    }
}
