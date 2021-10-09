using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Models;
using BandoriBot.Services;
using Newtonsoft.Json.Linq;
using Sora.Entities.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sora.Entities;
using Sora.Entities.CQCodes;
using Sora.Enumeration.EventParamsType;

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

    public struct Source
    {
        public DateTime time;
        public long FromGroup, FromQQ;
        public SoraApi Session;
        public bool IsTemp;

        internal static readonly HashSet<long> AdminQQs = new(File.ReadAllText("adminqq.txt").Split('\n').Select(long.Parse));

        public bool IsSuperadmin => AdminQQs.Contains(FromQQ);

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

        private static PermissionConfig cfg = Configuration.GetConfig<PermissionConfig>();

        private async Task<bool> CheckPermission(long target = 0,
            MemberRoleType required = MemberRoleType.Admin) =>
            (await Session.GetGroupMemberInfo(target, FromQQ)).memberInfo.Role >= required;

        public async Task<bool> HasPermission(string perm) => await HasPermission(perm, -1);
        public async Task<bool> HasPermission(string perm, long group) =>
            IsSuperadmin || perm == null ||
            cfg.t.ContainsKey(FromQQ) && (
            cfg.t[FromQQ].Contains($"*.{perm}") ||
            cfg.t[FromQQ].Contains($"{group}.{perm}")) ||
            perm.Contains('.') && await HasPermission(perm.Substring(0, perm.LastIndexOf('.')), group) ||
            perm != "*" && await HasPermission("*", group) || group > 0 && await CheckPermission(group);
    }

    public class MessageHandler : IMessageHandler
    {
        /*
        private class Message
        {
            public DateTime time;
            public int id;
            public long group, qq;
            public CQCode[] message;
        }*/

        private static readonly List<HandlerHolder> functions = new List<HandlerHolder>();
        internal static readonly IMessageHandler instance = new MessageHandler();
       // private static readonly Queue<Message> msgRecord = new Queue<Message>();
        private static readonly State head = new State();
        public static bool booted = false;

        public static SoraApi session;

        private class State
        {
            public State[] next = new State[256];
            public BlockingDelegate<CommandArgs> cmd;
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
            var @delegate = new BlockingDelegate<CommandArgs>(async args =>
            {
                if (!await args.Source.HasPermission(t.Permission, args.Source.FromGroup))
                {
                    await args.Callback("access denied.");
                    return;
                }
                if (!Configuration.GetConfig<BlacklistF>().InBlacklist(args.Source.FromGroup, t))
                    await t.Run(args);
            });

            foreach (var alias in t.Alias)
            {
                var node = head;
                foreach (var b in Encoding.UTF8.GetBytes(alias))
                {
                    if (node.next[b] == null) node.next[b] = new State();
                    node = node.next[b];
                }
                node.cmd = @delegate;
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

        private static Queue<int> msgqueue = new();
        public static readonly HashSet<long> bots = new();

        private static bool SameMessageFiltering(string msg, Source src)
        {
            if (bots.Contains(src.FromQQ)) return false;
            var hash = HashCode.Combine(msg, src.FromGroup, src.FromQQ, src.time);
            if (msgqueue.Contains(hash)) return false;
            msgqueue.Enqueue(hash);
            while (msgqueue.Count > 100) msgqueue.Dequeue();
            return true;
        }

        public static async Task OnMessage(SoraApi session, string message, Source Sender)
        {
            if (!booted) return;

            lock (msgqueue)
                if (!SameMessageFiltering(message, Sender)) return;

            long ticks = DateTime.Now.Ticks;

            Func<string, Task> callback = async s =>
            {
                try
                {
                    Utils.Log(LoggerLevel.Debug, $"[{ Sender.FromGroup}::{ Sender.FromQQ}] [{ (DateTime.Now.Ticks - ticks) / 10000}ms] sent msg: " + s);
                    if (Sender.FromGroup != 0)
                        await session.SendGroupMessage(Sender.FromGroup, Utils.GetMessageChain(s));
                    else
                        await session.SendPrivateMessage(Sender.FromQQ, Utils.GetMessageChain(s));

                }
                catch (Exception e)
                {
                    Utils.Log(LoggerLevel.Error, "error in msg: " + s + "\n" + e.ToString());
                }
            };

            RecordDatabaseManager.AddRecord(Sender.FromQQ, Sender.FromGroup, DateTime.Now, message);

            Utils.Log(LoggerLevel.Debug, $"[{Sender.FromGroup}::{Sender.FromQQ}]recv msg: " + message);

            await Task.Run(() => instance.OnMessage(new HandlerArgs
            {
                message = message,
                Sender = Sender,
                Callback = callback
            }));
        }

        private void ProcessError(Func<string, Task> callback, Exception e, bool senderr)
        {
            Utils.Log(LoggerLevel.Error, e.ToString());

            while (e is AggregateException) e = e.InnerException;
            if (e is ApiException) callback(e.Message);
            else if (senderr) callback(e.ToString());
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
                    ProcessError(args.Callback, e, await args.Sender.HasPermission("ignore.noerr", -1));
                }
            }

            foreach (HandlerHolder function in functions)
            {
                try
                {
                    if ((!cmdhandle || function.handler.IgnoreCommandHandled) && !Configuration.GetConfig<BlacklistF>().InBlacklist(args.Sender.FromGroup, function))
                        if (await function.cmd.Run(args)) break;
                }
                catch (Exception e)
                {
                    ProcessError(args.Callback, e, await args.Sender.HasPermission("ignore.noerr", -1));
                }
            }
            return true;
        }
        /*
#pragma warning disable CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
        public async Task<bool> GroupMessage(SoraApi session, IGroupMessageEventArgs e)
        {
            var source = e.Chain.First() as SourceMessage;
            if (source != null && (Configuration.GetConfig<Antirevoke>().hash.Contains(e.Sender.Group.Id) ||
                Configuration.GetConfig<AntirevokePlus>().hash.Contains(e.Sender.Group.Id)))
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

        public async Task<bool> BotInvitedJoinGroup(SoraApi session, IBotInvitedJoinGroupEventArgs e)
        {
            await session.HandleGroupApplyAsync(e, GroupApplyActions.Allow);
            return true;
        }

        public async Task<bool> GroupMessageRevoked(SoraApi session, IGroupMessageRevokedEventArgs e)
        {
            var record = msgRecord.FirstOrDefault(msg => msg.id == e.MessageId);
            if (record == null || e.Operator.Id != record.qq) return false;
            try
            {
                if (Configuration.GetConfig<AntirevokePlus>().hash.Contains(record.group))
                {
                    await session.SendFriendMessageAsync(Source.AdminQQs.First(), (new Element[]
                    {
                        new PlainMessage($"群{record.group}的{record.qq}尝试撤回一条消息：")
                    }).Concat(record.message).ToArray());
                }
                else
                {
                    await session.SendGroupMessageAsync(record.group, (new Element[]
                    {
                        new AtMessage(e.Operator.Id),
                        new PlainMessage("尝试撤回一条消息：")
                    }).Concat(record.message).ToArray());
                }
                return true;
            }
            catch (Exception e2)
            {
                this.Log(LoggerLevel.Error, e2.ToString());
                return false;
            }
        }*/
#pragma warning restore CS1998 // 异步方法缺少 "await" 运算符，将以同步方式运行
    }
}
