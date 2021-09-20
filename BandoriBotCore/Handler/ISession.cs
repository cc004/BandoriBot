using Sora.Entities.Base;

namespace BandoriBot.Handler
{
    public interface ISession
    {
        public SoraApi Session { get; set; }
    }
}
