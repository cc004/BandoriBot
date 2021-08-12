using System;
using BandoriBot.Config;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using TrClient;

namespace BandoriBot.Commands
{
    public class LoginCommand : ICommand
    {
        public List<string> Alias => new() {"/tr"};

        private class Session
        {
            public TClient client = new();
            public Action<string> callback;
            public DateTime lastActive;
            public bool abort = false;
            public Thread thread;
        }

        private Dictionary<long, Session> session = new();

        public async Task Run(CommandArgs args)
        {
            var splits = args.Arg.Trim().Split(' ');
            switch (splits[0])
            {
                case "exit":
                    if (session.TryGetValue(args.Source.FromQQ, out var ses))
                    {
                        session.Remove(args.Source.FromQQ);
                        ses.abort = true;
                    }

                    break;
                case "run":
                    if (session.TryGetValue(args.Source.FromQQ, out var ses2))
                    {
                        ses2.client.ChatText(string.Join(" ", splits.Skip(1)));
                        ses2.lastActive = DateTime.Now;
                    }

                    break;
                case "login":
                    if (splits.Length != 5)
                    {
                        await args.Callback("/tr login host:port protocol username password");
                        break;
                    }
                    if (session.ContainsKey(args.Source.FromQQ))
                    {
                        await args.Callback("你已经拥有一个session了！");
                        return;
                    }

                    var s = new Session();
                    s.callback = s => args.Callback(s).Wait();
                    s.lastActive = DateTime.Now;
                    s.client.OnChat += (_, text, _) => s.callback(text.ToString());
                    s.client.OnMessage += (_, text) => s.callback(text);
                    s.client.CurRelease = splits[2];
                    var addr = splits[1].Split(":");
                    var port = addr.Length == 1 ? 7777 : int.Parse(addr[1]);
                    var host = addr[0];
                    s.client.Username = splits[3];
                    s.client.shouldExit = () => s.abort || (DateTime.Now - s.lastActive) > TimeSpan.FromMinutes(1);
                    s.thread = new Thread(() =>
                    {
                        try
                        {
                            s.client.GameLoop(host, port, splits[4]);
                        }
                        catch (Exception)
                        {
                            args.Callback("exception in gameloop").Wait();
                        }
                    });
                    s.thread.Start();
                    session[args.Source.FromQQ] = s;
                    break;
            }
        }

    }
}
