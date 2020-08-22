using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public abstract class DictConfiguration<TKey, TValue> : JConfiguration<JObject> where TKey : IEquatable<TKey>
    {
        public TValue this[TKey key]
        {
            get
            {
                foreach (var obj in list)
                    if (obj.Value<TKey>("key").Equals(key))
                    {
                        return obj.Value<TValue>("value");
                    }
                return default;
            }
            set
            {
                foreach (var obj in list)
                    if (obj.Value<TKey>("key").Equals(key))
                    {
                        obj["value"] = JToken.FromObject(value);
                        return;
                    }
                list.Add(new JObject
                {
                    ["name"] = JToken.FromObject(key),
                    ["value"] = JToken.FromObject(value)
                });
            }
        }
    }
}
