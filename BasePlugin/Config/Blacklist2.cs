namespace BandoriBot.Config
{
    public class Blacklist2 : HashConfiguration<string>
    {
        public override string Name => "blacklist2.json";

        public bool InBlacklist(long qq)
        {
            return hash.Contains(qq.ToString());
        }
    }
}