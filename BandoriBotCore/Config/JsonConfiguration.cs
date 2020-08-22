using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
