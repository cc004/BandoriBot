using System;
using System.Collections.Generic;
using System.Text;

namespace BandoriBot.Config
{
    public class SubscribeConfig : SerializableConfiguration<Dictionary<string, CarType>>
    {
        public override string Name => "subscribe.json";

        public override void LoadDefault()
        {
            t = new Dictionary<string, CarType>();
        }

        public CarType this[string target]
        {
            get
            {
                lock (this)
                {
                    if (t.TryGetValue(target, out var type))
                        return type;
                    else
                        return CarType.None;
                }
            }
            set
            {
                lock (this)
                {
                    t[target] = value;
                    Save();
                }
            }
        }
    }
    }
