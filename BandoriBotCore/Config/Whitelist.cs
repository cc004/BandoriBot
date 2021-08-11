namespace BandoriBot.Config
{
    public class Whitelist : HashConfiguration<long>
    {
        public override string Name => "whitelist.json";
    }
}
