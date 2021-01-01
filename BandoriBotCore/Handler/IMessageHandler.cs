using BandoriBot.Models;
using System;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public struct HandlerArgs
    {
        public string message;
        public Source Sender;
        public Func<string, Task> Callback;
    }

    public interface IMessageHandler
    {
        bool IgnoreCommandHandled { get; }
        Task<bool> OnMessage(HandlerArgs args);
    }
}
