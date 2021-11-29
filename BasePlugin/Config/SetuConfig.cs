using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BandoriBot.Config
{
    public class R18Allowed : HashConfiguration<long>
    {
        public override string Name => "r18allowed.json";
    }
    public class NormalAllowed : HashConfiguration<long>
    {
        public override string Name => "normalallowed.json";
    }

    [JsonObject]
    public class Picture
    {
        public int pid, uid, p;
        public string title, author, url;
        public bool r18;
        public int width, height;
        public string[] tags;
    }

    public class SetuConfig : SerializableConfiguration<List<Picture>>
    {
        public override string Name => "setu.json";
    }
}
