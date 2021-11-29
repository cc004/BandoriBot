using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Models;
using BandoriBot.Services;
using Sora.Entities.Base;

namespace BandoriBot
{
    public static class MessageHandler
    {
        public static readonly HashSet<long> selfids =
            new(File.ReadAllText("selfid.txt").Split('\n').Select(long.Parse));
        private static readonly ConcurrentDictionary<long, (long, long)> hashedGroupCache = new ();
        private static readonly List<HandlerHolder> functions = new List<HandlerHolder>();
        private static readonly State head = new ();
        public static bool booted = false;

        public static SoraApi session => bots.OrderBy(p => p.GetHashCode()).First().Value;

        private static unsafe long GetHashCode(string str)
        {
            long num = 5381;
            long num2 = num;
            int num3;
            fixed (char* ptr = str)
            {
                char* ptr2 = ptr;
                while ((num3 = (int)(*ptr2)) != 0)
                {
                    num = ((num << 5) + num ^ num3);
                    num3 = (int)ptr2[1];
                    if (num3 == 0)
                    {
                        break;
                    }
                    num2 = ((num2 << 5) + num2 ^ num3);
                    ptr2 += 2;
                }
            }
            return num + num2 * 1566083941;
        }

        public static long HashGroupCache(long guild, long channel)
        {
            var res = GetHashCode($"{guild}{channel}");
             hashedGroupCache.TryAdd(res, (guild, channel));
             return res;
        }

        public static (long guild, long channel) GetGroupCache(long hash) => hashedGroupCache[hash];

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

        public static void SortHandler()
        {
            functions.Sort((a, b) => -a.handler.Priority.CompareTo(b.handler.Priority));
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
                {
                    Utils.Log(LoggerLevel.Info, $"message {args.Arg} triggered command {t.GetType().FullName}");
                    await t.Run(args);
                }
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
        
        //provide api compatibility

        public static void OnMessage(string message, Source source, bool isAdmin, Action<string> callback)
        {
            OnMessage(new HandlerArgs
            {
                message = message,
                Sender = source,
                Callback = async s => callback(s)
            }).Wait();
        }

        private static ConcurrentQueue<int> msgqueue = new();
        public static readonly Dictionary<long, SoraApi> bots = new();

        private static bool SameMessageFiltering(string msg, Source src)
        {
            lock (bots)
                if (bots.ContainsKey(src.FromQQ)) return false;
            var hash = HashCode.Combine(msg, src.FromGroup, src.FromQQ, src.time.ToTimestamp() / 3000);
            if (msgqueue.Contains(hash)) return false;
            msgqueue.Enqueue(hash);
            while (msgqueue.Count > 100) msgqueue.TryDequeue(out _);
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
                    if (Sender.IsGuild)
                    {
                        var (guild, channel) = GetGroupCache(Sender.FromGroup);
                        Utils.Log(LoggerLevel.Debug,
                            $"[{guild}::{channel}::{Sender.FromQQ}] [{(DateTime.Now.Ticks - ticks) / 10000}ms] sent msg: " +
                            s);
                        await session.SendGuildMessage(guild, channel, Utils.GetMessageChain(s));
                    }
                    else
                    {
                        Utils.Log(LoggerLevel.Debug,
                            $"[{Sender.FromGroup}::{Sender.FromQQ}] [{(DateTime.Now.Ticks - ticks) / 10000}ms] sent msg: " +
                            s);
                        if (Sender.FromGroup != 0)
                            await session.SendGroupMessage(Sender.FromGroup, Utils.GetMessageChain(s));
                        else
                            await session.SendPrivateMessage(Sender.FromQQ, Utils.GetMessageChain(s));
                    }

                }
                catch (Exception e)
                {
                    Utils.Log(LoggerLevel.Error, $"error in msg: {s}\n{e}");
                }
            };

            RecordDatabaseManager.AddRecord(Sender.FromQQ, Sender.FromGroup, DateTime.Now, message);

            Utils.Log(LoggerLevel.Debug, $"[{Sender.FromGroup}::{Sender.FromQQ}]recv msg: " + message);

            await Task.Run(() => OnMessage(new HandlerArgs
            {
                message = message,
                Sender = Sender,
                Callback = callback
            }));
        }

        private static void ProcessError(Func<string, Task> callback, Exception e, bool senderr)
        {
            Utils.Log(LoggerLevel.Error, e.ToString());

            while (e is AggregateException) e = e.InnerException;
            if (e is ApiException) callback(e.Message);
            else if (senderr) callback(e.ToString());
        }

        public static async Task<bool> OnMessage(HandlerArgs args)
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
                    {
                        if (await function.cmd.Run(args))
                        {
                            break;
                        }
                    }
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
        public static async Task Broadcast(string msg)
        {
            var group = await GetGroupList();
            foreach (var tuple in group.GroupBy(t => t.Item1).Select(g => g.OrderBy(g => g.GetHashCode()).First()))
                await tuple.Item2.SendGroupMessage(tuple.Item1, Utils.GetMessageChain(msg));
        }

        public static async Task<(long, SoraApi)[]> GetGroupList()
        {
            var group = new List<(long, SoraApi)>();
            foreach (var pair in bots)
                group.AddRange((await pair.Value.GetGroupList()).groupList.Select(g => (g.GroupId, pair.Value)));
            return group.ToArray();
        }
    }
}
