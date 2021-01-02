using Newtonsoft.Json.Linq;
using System;

namespace BandoriBot.DataStructures
{
    public class Car : IComparable<Car>
    {
        public DateTime time;
        public string rawmessage;
        public int index;
        private static DateTime ToDateTime(long timestamp)
        {
            return new DateTime(1970, 1, 1).AddTicks(timestamp * 10000).ToLocalTime();
        }

        public Car() { }
        public Car(JObject obj)
        {
            time = ToDateTime((long)obj["time"]);
            rawmessage = (string)obj["raw_message"];
            index = int.Parse((string)obj["number"]);
        }

        public int CompareTo(Car other)
        {
            return other.time.CompareTo(time);
        }

        public override string ToString()
        {
            return rawmessage +
                   $"({(int)(DateTime.Now - time).TotalSeconds}취품)";
        }

        public string ToString(DateTime now)
        {
            return rawmessage +
                   $"({(int)(now - time).TotalSeconds}취품)";
        }
    }
}
