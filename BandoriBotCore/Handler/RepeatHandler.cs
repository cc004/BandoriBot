using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public class RepeatHandler : IMessageHandler
    {
        public static RepeatHandler Instance;

        class GroupStatus
        {
            public int messageHash;
            public bool isRepeated;
        }
        private Dictionary<int, GroupStatus> lastMessage = new Dictionary<int, GroupStatus>();
        private const string breakMessage = "打断", breakMessage2 = "继续打断";

        public bool IgnoreCommandHandled => true;

        public RepeatHandler()
        {
            Instance = this;
        }

        public void ClearMsg(Source source)
        {
            var hash = source.FromGroup == 0 ? source.FromQQ.GetHashCode() : $"{source.FromGroup}".GetHashCode();
            if (lastMessage.ContainsKey(hash))
                lastMessage.Remove(hash);
        }

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            return false;
            int groupHash = args.Sender.FromGroup == 0 ? args.Sender.FromQQ.GetHashCode() : $"{args.Sender.FromGroup}".GetHashCode();
            int messageHash = args.message.GetHashCode();

            if (lastMessage.TryGetValue(groupHash, out GroupStatus status))
            {
                if (status.messageHash == messageHash)
                {
                    if (!status.isRepeated)
                    {
                        string output =
                            new Random().NextDouble() < 0.5 ?
                            (args.message == breakMessage ? breakMessage2 : breakMessage) :
                            args.message;
                        await args.Callback(output);
                        status.messageHash = output.GetHashCode();
                        status.isRepeated = output == args.message;
                    }
                }
                else
                {
                    status.messageHash = messageHash;
                    status.isRepeated = false;
                }
            }
            else
                lastMessage.Add(groupHash, new GroupStatus
                {
                    messageHash = messageHash,
                    isRepeated = false
                });
            return false;
        }
    }
}
