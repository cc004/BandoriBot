using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    public abstract class Cooldown : JConfiguration<JObject>
    {
        public void Set(long id, TimeSpan expire)
        {
            var obj = new JObject
            {
                ["id"] = id,
                ["expire"] = DateTime.Now + expire
            };
            for (int i = 0; i < list.Count; ++i)
                if ((long) list[i]["id"] == id)
                {
                    list[i] = obj;
                    Save();
                    return;
                }
            list.Add(obj);
            Save();
        }

        public bool IsExpire(long id)
        {
            for (int i = 0; i < list.Count; ++i)
                if ((long)list[i]["id"] == id)
                    return (DateTime)list[i]["expire"] < DateTime.Now;
            return true;
        }
    }
}
