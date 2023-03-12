using BandoriBot;
using BandoriBot.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BasePlugin.Commands
{
    public class Suoniuzi : ICommand
    {
        public List<string> Alias => new() { "/snz" };

        public async Task Run(CommandArgs args)
        {
            var from = args.Source.FromQQ;
            var group = args.Source.FromGroup;
            var a = args.Arg.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var target = long.Parse(a[0]);
            var message = a[1];

            lock (SuoniuziHandler.messageHandling)
            SuoniuziHandler.messageHandling.Add(target, () => args.Source.Session.SendGroupMessage(
                group, Utils.GetMessageChain(message)).AsTask().Wait());

            this.Log(BandoriBot.Models.LoggerLevel.Info, $"message {group}@{message} registered to {target}");

            await args.Callback(message);
        }
    }
    public class SuoniuziHandler : IMessageHandler
    {
        internal static Dictionary<long, Action> messageHandling = new Dictionary<long, Action>();

        private static Dictionary<Action, DateTime> message = new Dictionary<Action, DateTime>();

        public float Priority => 999;
        public bool IgnoreCommandHandled => true;

        private static readonly Regex reg = new Regex(@"\[mirai:at=(\d+)\].*你已经嗦不动了喵.*等待(\d+(\.\d*)?)秒后再嗦喵");

        static SuoniuziHandler()
        {
            new Thread(() =>
            {
                for (; ; )
                {
                    Thread.Sleep(50);

                    bool flag = false;
                    var now = DateTime.Now;

                    lock (message)
                    {
                        foreach (var pair in message)
                        {
                            if (pair.Value <= now)
                            {
                                Utils.Log(BandoriBot.Models.LoggerLevel.Info, $"message {pair.Key} sent due to {pair.Value} > {now}");
                                pair.Key();
                                flag = true;
                            }
                        }
                    }

                    if (flag)
                        Thread.Sleep(400);
                }

            }).Start();
        }

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            var match = reg.Match(args.message);
            if (!match.Success) return false;

            var toqq = long.Parse(match.Groups[1].Value);
            var time = float.Parse(match.Groups[2].Value);

            lock (messageHandling)
            {
                if (!messageHandling.TryGetValue(toqq, out var msg)) return false;

                var now = DateTime.Now;

                lock (message)
                {
                    message[msg] = now.AddSeconds(time);
                    this.Log(BandoriBot.Models.LoggerLevel.Info, $"message {msg} changed to {message[msg]}");
                }
            }

            return true;
        }
    }
}
