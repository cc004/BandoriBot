using Newtonsoft.Json.Linq;

namespace BandoriBot.Config
{
    public class Save : DictConfiguration<long, JObject>
    {
        public override string Name => "save.json";
    }
}
