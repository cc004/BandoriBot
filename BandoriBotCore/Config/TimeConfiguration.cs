
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    [JsonObject]
    public class Schedule
    {
        public int delay;
        public string message;
        public long group;
    }

    class TimeConfiguration : SerializableConfiguration<List<Schedule>>
    {
        public override string Name => "schedules.json";

        public override void LoadDefault()
        {
            t = new List<Schedule>();
        }
    }
}
