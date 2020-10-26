using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public abstract class HashConfiguration<TValue> : SerializableConfiguration<List<TValue>>
    {
        public HashSet<TValue> hash = new HashSet<TValue>();

        public sealed override void LoadDefault()
        {
            hash = new HashSet<TValue>();

            t = new List<TValue>();
        }

        public sealed override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            hash = new HashSet<TValue>(t);
        }

        public sealed override void SaveTo(BinaryWriter bw)
        {
            t = hash.ToList();
            base.SaveTo(bw);
        }
    }
}
