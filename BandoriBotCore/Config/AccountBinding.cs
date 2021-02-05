using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public class Binding
    {
        public string username;
        public long qq;
        public int group;
    }

    class AccountBinding : SerializableConfiguration<List<Binding>>
    {
        public override string Name => "account.json";
    }
}
