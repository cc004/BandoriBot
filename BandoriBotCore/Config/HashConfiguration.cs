using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public abstract class HashConfiguration : SerializableConfiguration<List<long>>
    {
        public HashSet<long> hash = new HashSet<long>();

        public sealed override void LoadDefault()
        {
            hash = new HashSet<long>
            {
                0
            };

            t = new List<long>
            {
                0
            };
        }

        public sealed override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            hash = new HashSet<long>(t);
        }

        public sealed override void SaveTo(BinaryWriter bw)
        {
            t = hash.ToList();
            base.SaveTo(bw);
        }
    }
}
