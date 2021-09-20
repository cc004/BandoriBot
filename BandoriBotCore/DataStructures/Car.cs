using Newtonsoft.Json.Linq;
using System;

namespace BandoriBot.DataStructures
{
    public class Car : IComparable<Car>
    {
        internal static long DeltaTime { private get; set; }

        public DateTime time;
        public string rawmessage;
        public int index;

        public Car() { }
        public Car(JObject obj)
        {
            time = ((long)obj["time"] - DeltaTime).ToDateTime();
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
