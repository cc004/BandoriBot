using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Models;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Mirai_CSharp.Plugin.Interfaces;
using Newtonsoft.Json.Linq;
using SekaiClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public struct Source
    {
        public long FromGroup, FromQQ;
        public MiraiHttpSession Session;
        public bool IsTemp;

        private static readonly long AdminQQ = 1176321897;

        private bool IsAdmin => AdminQQ == FromQQ || Configuration.GetConfig<Admin>().hash.Contains(FromQQ);

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

        public async Task<bool> CheckPermission(long target = 0, GroupPermission required = GroupPermission.Administrator)
        {
            var qq = FromQQ;
            return IsAdmin || ((target == 0 ? new IGroupMemberInfo[0] : await Session.GetGroupMemberListAsync(target))
                .SingleOrDefault(info => info.Id == qq)?.Permission ?? GroupPermission.Member) > required;
        }
    }

    public class MessageHandler : IMessageHandler, IFriendMessage, IGroupMessage, IBotInvitedJoinGroup, INewFriendApply, IGroupMessageRevoked, ITempMessage
    {
        private class Message
        {
            public DateTime time;
            public int id;
            public long group, qq;
            public IMessageBase[] message;
        }

        private static readonly List<HandlerHolder> functions = new List<HandlerHolder>();
        private static readonly IMessageHandler instance = new MessageHandler();
        private static readonly Queue<Message> msgRecord = new Queue<Message>();
        private static readonly State head = new State();
        public static bool booted = false;

        private class State
        {
            public State[] next = new State[256];
            public BlockingDelegate<CommandArgs, Task> cmd;
        }

        private class HandlerHolder
        {
            public IMessageHandler handler;
            public BlockingDelegate<HandlerArgs, Task<bool>> cmd;

            public HandlerHolder(IMessageHandler handler)
            {
                this.handler = handler;
                cmd = new BlockingDelegate<HandlerArgs, Task<bool>>(handler.OnMessage);
            }
        }

        public static void Register<T>() where T : new()
        {
            var t = new T();
            if (t is ICommand tcmd) Register(tcmd);
            else if (t is IMessageHandler tmsg) Register(tmsg);
        }

        public static void Register(IMessageHandler t)
        {
            functions.Add(new HandlerHolder(t));
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
                node.cmd = new BlockingDelegate<CommandArgs, Task>(async args =>
                {
                    if (!Configuration.GetConfig<BlacklistF>().InBlacklist(args.Source.FromGroup, t))
                        await t.Run(args);
                });
            }
        }

        public bool IgnoreCommandHandled => true;

        //provide api compatibility

        public void OnMessage(string message, Source source, bool isAdmin, Action<string> callback)
        {
            OnMessage(new HandlerArgs
            {
                message = message,
                Sender = source,
                Callback = async s => callback(s)
            }).Wait();
        }

        protected static void OnMessage(MiraiHttpSession session, string message, Source Sender)
        {
            if (!booted) return;

            long ticks = DateTime.Now.Ticks;

            Func<string, Task> callback = async s =>
            {
                try
                {
                    Utils.Log(LoggerLevel.Debug, $"[{(DateTime.Now.Ticks - ticks) / 10000}ms] sent msg: " + s);
                    if (Sender.FromGroup != 0)
                        await session.SendGroupMessageAsync(Sender.FromGroup, await Utils.GetMessageChain(s, async p => await session.UploadPictureAsync(UploadTarget.Group, p)));
                    else if (!Sender.IsTemp)
                        await session.SendFriendMessageAsync(Sender.FromQQ, await Utils.GetMessageChain(s, async p => await session.UploadPictureAsync(UploadTarget.Temp, p)));
                    else
                        await session.SendTempMessageAsync(Sender.FromQQ, Sender.FromGroup, await Utils.GetMessageChain(s, async p => await session.UploadPictureAsync(UploadTarget.Friend, p)));

                }
                catch (Exception e)
                {
                    Utils.Log(LoggerLevel.Error, "error in msg: " + s + "\n" + e.ToString());
                }
            };

            Utils.Log(LoggerLevel.Debug, "recv msg: " + message);

            Task.Run(() => instance.OnMessage(new HandlerArgs
            {
                message = message,
                Sender = Sender,
                Callback = callback
            })).Start();
        }

        private void ProcessError(Func<string, Task> callback, Exception e)
        {
            Utils.Log(LoggerLevel.Error, e.ToString());

            while (e is AggregateException) e = e.InnerException;
            if (e is ApiException) callback(e.Message);
        }

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            var node = head;
            var i = 0;
            var bytes = Encoding.UTF8.GetBytes(args.message);

            foreach (var b in bytes)
            {
                var next = node.next[b];
                if (next == null) break;
                node = next;
                ++i;
            }

            var cmdhandle = false;

            if (node.cmd != null)
            {
                try
                {
                    await node.cmd.Run(new CommandArgs
                    {
                        Arg = args.message.Substring(Encoding.UTF8.GetString(bytes.Take(i).ToArray()).Length),
                        Source = args.Sender,
                        Callback = args.Callback
                    });
                    cmdhandle = true;
                }
                catch (Exception e)
                {
                    ProcessError(args.Callback, e);
                }
            }

            foreach (HandlerHolder function in functions)
            {
                try
                {
                    if ((!cmdhandle || function.handler.IgnoreCommandHandled) && !Configuration.GetConfig<BlacklistF>().InBlacklist(args.Sender.FromGroup, function))
                         if (await await function.cmd.Run(args)) break;
                }
                catch (Exception e)
                {
                    ProcessError(args.Callback, e);
                }
            }
            return true;
        }

#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
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

            OnMessage(session, Utils.GetCQMessage(e.Chain), new Source
            {
                FromGroup = e.Sender.Group.Id,
                FromQQ = e.Sender.Id,
                Session = session
            });
            return false;
        }

        public async Task<bool> FriendMessage(MiraiHttpSession session, IFriendMessageEventArgs e)
        {
            OnMessage(session, Utils.GetCQMessage(e.Chain), new Source
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
            OnMessage(session, Utils.GetCQMessage(e.Chain), new Source
            {
                FromGroup = 0,
                FromQQ = e.Sender.Id,
                Session = session,
                IsTemp = true
            });
            return false;
        }
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
    }
}
