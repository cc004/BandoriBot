using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Models;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public struct Source
    {
        public long FromGroup, FromQQ;
        public MiraiHttpSession Session;
    }

    public delegate void ResponseCallback(string message);
    public class MessageHandler : IMessageHandler, IFriendMessage, IGroupMessage, IBotInvitedJoinGroup, INewFriendApply, IGroupMessageRevoked
    {
        private class Message
        {
            public DateTime time;
            public int id;
            public long group, qq;
            public IMessageBase[] message;
        }

        //TODO: gather the commands together to improve speed.
        private static readonly List<IMessageHandler> functions = new List<IMessageHandler>();
        private static readonly IMessageHandler instance = new MessageHandler();
        private static readonly Queue<Message> msgRecord = new Queue<Message>();
        public static bool booted = false;

        public static void Register<T>(T t) where T : IMessageHandler
        {
            functions.Add(t);
        }

        private static long AdminQQ = 1176321897;

        private static bool IsIgnore(Source sender)
        {
            return false;
        }

        protected static async Task OnMessage(MiraiHttpSession session, string message, Source Sender)
        {
            if (!booted) return;

            bool isAdmin = AdminQQ == Sender.FromQQ || Configuration.GetConfig<Admin>().hash.Contains(Sender.FromQQ);

            if (IsIgnore(Sender)) return;

            ResponseCallback callback = delegate (string s)
            {
                try
                {
                    if (Sender.FromGroup != 0)
                        session.SendGroupMessageAsync(Sender.FromGroup, Utils.GetMessageChain(s)).Wait();
                    else
                        session.SendFriendMessageAsync(Sender.FromQQ, Utils.GetMessageChain(s)).Wait();

                    Utils.Log(LoggerLevel.Debug, "sent msg: " + s);
                }
                catch (Exception e)
                {
                    Utils.Log(LoggerLevel.Error, "error in msg: " + s + "\n" + e.ToString());
                }
            };

            Utils.Log(LoggerLevel.Debug, "recv msg: " + message);
            instance.OnMessage(message, Sender, isAdmin, callback);
        }

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            foreach (IMessageHandler function in functions)
            {
                try
                {
                    if (function.OnMessage(message, Sender, isAdmin, callback)) break;
                }
                catch (Exception e)
                {
                    callback($"Unhandled exception : {e}");
                }
            }
            return true;
        }

        public async Task<bool> GroupMessage(MiraiHttpSession session, IGroupMessageEventArgs e)
        {
            var source = e.Chain.First() as SourceMessage;
            if (source != null && (Configuration.GetConfig<Antirevoke>().hash.Contains(e.Sender.Group.Id) || e.Sender.Group.Id == 708647018))
            {
                var now = DateTime.Now;
                while (msgRecord.Count > 0)
                {
                    if (now - msgRecord.Peek().time > new TimeSpan(0, 10, 0))
                        msgRecord.Dequeue();
                    else
                        break;
                }

                msgRecord.Enqueue(new Message
                {
                    id = source.Id,
                    message = e.Chain.Skip(1).ToArray(),
                    group = e.Sender.Group.Id,
                    qq = e.Sender.Id,
                    time = now
                });
            }

            await OnMessage(session, Utils.GetCQMessage(e.Chain), new Source
            {
                FromGroup = e.Sender.Group.Id,
                FromQQ = e.Sender.Id,
                Session = session
            });
            return false;
        }

        public async Task<bool> FriendMessage(MiraiHttpSession session, IFriendMessageEventArgs e)
        {
            await OnMessage(session, Utils.GetCQMessage(e.Chain), new Source
            {
                FromGroup = 0,
                FromQQ = e.Sender.Id,
                Session = session
            });
            return false;
        }

        public async Task<bool> NewFriendApply(MiraiHttpSession session, INewFriendApplyEventArgs e)
        {
            await session.HandleNewFriendApplyAsync(e, FriendApplyAction.Allow);
            return true;
        }

        public async Task<bool> BotInvitedJoinGroup(MiraiHttpSession session, IBotInvitedJoinGroupEventArgs e)
        {
            await session.HandleGroupApplyAsync(e, GroupApplyActions.Allow);
            return true;
        }

        public async Task<bool> GroupMessageRevoked(MiraiHttpSession session, IGroupMessageRevokedEventArgs e)
        {
            var record = msgRecord.FirstOrDefault(msg => msg.id == e.MessageId);
            if (record == null || e.Operator.Id != record.qq) return false;
            try
            {
                await session.SendGroupMessageAsync(record.group, (new IMessageBase[]
                {
                    new AtMessage(e.Operator.Id, ""),
                    new PlainMessage("尝试撤回一条消息：")
                }).Concat(record.message).ToArray());
                return true;
            }
            catch (Exception e2)
            {
                this.Log(LoggerLevel.Error, e2.ToString());
                return false;
            }
        }
    }
}
