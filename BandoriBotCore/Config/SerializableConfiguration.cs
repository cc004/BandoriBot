using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public abstract class SerializableConfiguration<T> : Configuration
    {
        public T t;

        public override void LoadFrom(BinaryReader br)
        {
            using var sr = new StreamReader(br.BaseStream);
            t = JsonConvert.DeserializeObject<T>(sr.ReadToEnd());
        }

        public override void SaveTo(BinaryWriter bw)
        {
            using var sw = new StreamWriter(bw.BaseStream);
            sw.Write(JsonConvert.SerializeObject(t, Formatting.Indented));
        }
    }
}
