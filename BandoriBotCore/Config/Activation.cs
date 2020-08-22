using System.Collections.Generic;
using System.IO;

namespace BandoriBot.Config
{
    class Activation : SerializableConfiguration<Dictionary<long, bool>>
    {
        private Dictionary<long, bool> activation => t;

        public bool this[long index]
        {
            get
            {
                if (!activation.ContainsKey(index))
                {
                    activation.Add(index, true);
                    Save();
                }
                return activation[index];
            }
            set
            {
                if (activation.ContainsKey(index))
                    activation[index] = value;
                else
                    activation.Add(index, value);
                Save();
            }
        }
        public override string Name => "activation.json";

        public override void LoadDefault()
        {
            t = new Dictionary<long, bool>();
        }

    }
}
