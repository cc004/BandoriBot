using Newtonsoft.Json;
using System;
using System.IO;

namespace BandoriBot.Config
{
    public abstract class SerializableConfiguration<T> : Configuration
    {
        public T t;

        public override void LoadDefault()
        {
            t = Activator.CreateInstance<T>();
        }

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
