using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Models;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public struct Source
    {
        public long FromGroup, FromQQ;
        public MiraiHttpSession Session;
        public bool IsTemp;

        public JObject GetSave()
        {
            var result = Configuration.GetConfig<Save>()[FromQQ];
            if (result == null)
            {
                result = new JObject();
                Configuration.GetConfig<Save>()[FromQQ] = result;
            }
            return result;
        }
    }

    public delegate void ResponseCallback(string message);
    public class MessageHandler : IMessageHandler, IFriendMessage, IGroupMessage, IBotInvitedJoinGroup, INewFriendApply, IGroupMessageRevoked, ITempMessage
    {
        private class Message
        {
            public DateTime time;
            public int id;
            public long group, qq;
            public IMessageBase[] message;
        }

        private static readonly List<IMessageHandler> functions = new List<IMessageHandler>();
        private static readonly IMessageHandler instance = new MessageHandler();
        private static readonly Queue<Message> msgRecord = new Queue<Message>();
        private static readonly State head = new State();
        public static bool booted = false;

        private class State
        {
            public State[] next = new State[256];
            public Action<CommandArgs> cmd;
        }

        public static void Register(ICommand t)
        {
            foreach (var alias in t.Alias)
            {
                var node = head;
                foreach (var b in Encoding.UTF8.GetBytes(alias))
                {
                    if (node.next[b] == null) node.next[b] = new State();
                    node = node.next[b];
                }
                node.cmd = t.Run;
            }
        }

        public static void Register(IMessageHandler t)
        {
            functions.Add(t);
        }

        private static long AdminQQ = 1176321897;

        private static bool IsIgnore(Source sender)
        {
            return false;
        }

#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        protected static async Task OnMessage(MiraiHttpSession session, string message, Source Sender)
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        {
            if (!booted) return;

            bool isAdmin = AdminQQ == Sender.FromQQ || Configuration.GetConfig<Admin>().hash.Contains(Sender.FromQQ);
            long ticks = DateTime.Now.Ticks;

            if (IsIgnore(Sender)) return;

            ResponseCallback callback = delegate (string s)
            {
                try
                {
                    Utils.Log(LoggerLevel.Debug, $"[{(DateTime.Now.Ticks - ticks) / 10000}ms] sent msg: " + s);
                    if (Sender.FromGroup != 0)
                        session.SendGroupMessageAsync(Sender.FromGroup, Utils.GetMessageChain(s, p => session.UploadPictureAsync(UploadTarget.Group, p).Result)).Wait();
                    else if (!Sender.IsTemp)
                        session.SendFriendMessageAsync(Sender.FromQQ, Utils.GetMessageChain(s, p => session.UploadPictureAsync(UploadTarget.Temp, p).Result)).Wait();
                    else
                        session.SendTempMessageAsync(Sender.FromQQ, Sender.FromGroup, Utils.GetMessageChain(s, p => session.UploadPictureAsync(UploadTarget.Friend, p).Result)).Wait();

                }
                catch (Exception e)
                {
                    Utils.Log(LoggerLevel.Error, "error in msg: " + s + "\n" + e.ToString());
                }
            };

            Utils.Log(LoggerLevel.Debug, "recv msg: " + message);

            Task.Run(() =>
            {
                instance.OnMessage(message, Sender, isAdmin, callback);
            }).Start();
        }

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            var node = head;
            var i = 0;
            var bytes = Encoding.UTF8.GetBytes(message);

            foreach (var b in bytes)
            {
                var next = node.next[b];
                if (next == null) break;
                node = next;
                ++i;
            }

            if (node.cmd != null)
            {
                try
                {
                    lock (node)
                    node.cmd(new CommandArgs
                    {
                        Arg = message.Substring(Encoding.UTF8.GetString(bytes.Take(i).ToArray()).Length),
                        Source = Sender,
                        IsAdmin = isAdmin,
                        Callback = callback
                    });
                }
                catch (Exception e)
                {
                    //callback($"Unhandled exception : {e}");
                    Utils.Log(LoggerLevel.Error, e.ToString());
                }
                return true;
            }

            foreach (IMessageHandler function in functions)
            {
                try
                {
                    lock (function)
                    if (function.OnMessage(message, Sender, isAdmin, callback)) break;
                }
                catch (Exception e)
                {
                    //callback($"Unhandled exception : {e}");
                    Utils.Log(LoggerLevel.Error, e.ToString());
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
                    new AtMessage(e.Operator.Id),
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

        public async Task<bool> TempMessage(MiraiHttpSession session, ITempMessageEventArgs e)
        {
            await OnMessage(session, Utils.GetCQMessage(e.Chain), new Source
            {
                FromGroup = 0,
                FromQQ = e.Sender.Id,
                Session = session,
                IsTemp = true
            });
            return false;
        }
    }
}
