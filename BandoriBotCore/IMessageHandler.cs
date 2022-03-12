using System;
using System.Threading.Tasks;

namespace BandoriBot
{
    public class HandlerArgs
    {
        public string message;
        public Source Sender;
        public Func<string, Task> Callback;
        public Task finishedTask = Task.CompletedTask;
    }

    public interface IMessageHandler
    {
        bool IgnoreCommandHandled { get; }
        float Priority => 0;
        Task<bool> OnMessage(HandlerArgs args);
    }
}
