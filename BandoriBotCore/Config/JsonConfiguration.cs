using Newtonsoft.Json.Linq;
using System.IO;

namespace BandoriBot.Config
{
    public abstract class JsonConfiguration : Configuration
    {
        public JToken json;

        public override void LoadFrom(BinaryReader br)
        {
            using var sr = new StreamReader(br.BaseStream);
            json = JToken.Parse(sr.ReadToEnd());
        }

        public override void SaveTo(BinaryWriter bw)
        {
            using var sw = new StreamWriter(bw.BaseStream);
            sw.Write(json.ToString());
        }
    }
}
