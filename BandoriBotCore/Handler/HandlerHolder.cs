using BandoriBot.Services;

namespace BandoriBot.Handler
{
    public class HandlerHolder
    {
        public IMessageHandler handler;
        public BlockingDelegate<HandlerArgs, bool> cmd;

        public HandlerHolder(IMessageHandler handler)
        {
            this.handler = handler;
            cmd = new BlockingDelegate<HandlerArgs, bool>(handler.OnMessage);
        }
    }
}