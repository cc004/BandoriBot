using System.Collections.Generic;

namespace BandoriBot.Config
{
    class Delay : SerializableConfiguration<Dictionary<long, int>>
    {
        private Dictionary<long, int> delay => t;

        public int this[long index]
        {
            get
            {
                if (!delay.ContainsKey(index))
                {
                    delay.Add(index, 0);
                    Save();
                }
                return delay[index];
            }
            set
            {
                if (delay.ContainsKey(index))
                    delay[index] = value;
                else
                    delay.Add(index, value);
                Save();
            }
        }
        public override string Name => "delay.json";

        public override void LoadDefault()
        {
            t = new Dictionary<long, int>();
        }
    }
}
