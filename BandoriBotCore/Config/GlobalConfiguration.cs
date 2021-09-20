using Newtonsoft.Json;

namespace BandoriBot.Config
{
    [JsonObject]
    public class Global
    {
        public long group;
        public string func1Info;
        public string func5Info;
        public string func61Info;
        public string func62Info;
        public string func63Info;
        public bool func1Enabled;
        public bool func2Enabled;
    }

    public class GlobalConfiguration : SerializableConfiguration<Global>
    {
        public static Global Global => GetConfig<GlobalConfiguration>().t;

        public override string Name => "global.json";

        public override void LoadDefault()
        {
            t = new Global();
        }
    }
}
