using System.Collections.Generic;

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
