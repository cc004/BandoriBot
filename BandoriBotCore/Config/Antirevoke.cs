namespace BandoriBot.Config
{
    public class Antirevoke : HashConfiguration<long>
    {
        public override string Name => "antirevoke.json";
    }
    public class AntirevokePlus : HashConfiguration<long>
    {
        public override string Name => "antirevokeplus.json";
    }
}
