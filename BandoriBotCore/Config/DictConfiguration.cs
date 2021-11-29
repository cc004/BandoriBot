using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BandoriBot.Config
{
    [JsonObject]
    public class Pair<TKey, TValue>
    {
        public TKey key;
        public TValue value;
    }

    public abstract class DictConfiguration<TKey, TValue> : SerializableConfiguration<List<Pair<TKey, TValue>>> where TKey : IEquatable<TKey>
    {
        public override void LoadDefault()
        {
            t = new List<Pair<TKey, TValue>>();
        }

        public TValue this[TKey key]
        {
            get
            {
                foreach (var obj in t)
                    if (obj.key.Equals(key))
                    {
                        return obj.value;
                    }
                return default;
            }
            set
            {
                foreach (var obj in t)
                    if (obj.key.Equals(key))
                    {
                        obj.value = value;
                        return;
                    }
                t.Add(new Pair<TKey, TValue>
                {
                    key = key,
                    value = value
                });
            }
        }
    }
}
