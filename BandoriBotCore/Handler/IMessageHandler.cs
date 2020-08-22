using BandoriBot.Models;

namespace BandoriBot.Handler
{
    public interface IMessageHandler
    {
        bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback);
    }
}
