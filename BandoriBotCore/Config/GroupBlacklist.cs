namespace BandoriBot.Config
{
    public class GroupBlacklist : HashConfiguration<string>
    {
        public override string Name => "groupblacklist.json";
        public bool InBlacklist(long group)
        {
            return hash.Contains(group.ToString());
        }
    }
}