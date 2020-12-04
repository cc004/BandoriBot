using System;
using System.Collections.Generic;

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
        public bool OnMessage(string message, Source Sender, bool isAdmin, Action<string> callback)
        {
            int groupHash = Sender.FromGroup == 0 ? Sender.FromQQ.GetHashCode() : $"{Sender.FromGroup}".GetHashCode();
            int messageHash = message.GetHashCode();

            if (lastMessage.TryGetValue(groupHash, out GroupStatus status))
            {
                if (status.messageHash == messageHash)
                {
                    if (!status.isRepeated)
                    {
                        string output =
                            new Random().NextDouble() < 0.5 ?
                            (message == breakMessage ? breakMessage2 : breakMessage) :
                            message;
                        callback(output);
                        status.messageHash = output.GetHashCode();
                        status.isRepeated = output == message;
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
