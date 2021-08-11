using System.Collections.Generic;
using System.IO;

namespace BandoriBot.Config
{
    public abstract class JConfiguration<T> : SerializableConfiguration<List<T>>
    {
        public List<T> list = new List<T>();

        public sealed override void LoadDefault()
        {
            list = new List<T>();
            t = list;
        }

        public sealed override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            list = t;
        }

        public sealed override void SaveTo(BinaryWriter bw)
        {
            t = list;
            base.SaveTo(bw);
        }
    }
}
