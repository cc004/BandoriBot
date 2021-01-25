using BandoriBot.Config;
using BandoriBot.Handler;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Terraria
{
    public sealed class MainServer : IDisposable
    {
        private TcpListener listener;
        private readonly List<GameServer> clients = new List<GameServer>();
        private string format;
        private static Regex replace = new Regex(@"\$((?!&).|&...;)", RegexOptions.Compiled);
        private uint color;
        private Thread listenthread;

        public event Action<string> OnServerMessage;
        private static string FitReply(string format, string message, Source sender)
            => replace.Replace(format, m =>
            {
                var c = m.Value[1];
                if (c == 'm') return message;
                else if (c == '$') return "$";
                else if (c == 'g') return sender.FromGroup.ToString();
                else if (c == 'q') return sender.FromQQ.ToString();
                else if (c == 'c') return sender.GetName().Result;
                return m.Value;
            });


        private void Listening()
        {
            this.Log(Models.LoggerLevel.Info, "main server is accepting");
            try
            {
                while (true)
                {
                    var client = listener.AcceptTcpClient();
                    this.Log(Models.LoggerLevel.Debug, $"accepted socket from {client.Client.RemoteEndPoint}");
                    var svr = new GameServer(client);
                    svr.OnMessage += (msg, clr) =>
                    {
                        lock (clients)
                            foreach (var client in clients)
                                if (client != svr) client.SendMsg(msg, clr);

                        OnServerMessage?.Invoke(msg);
                    };

                    lock (clients)
                    {
                        clients.RemoveAll(s => !s.Valid);
                        clients.Add(svr);
                    }

                }
            }
            catch
            {

            }
        }

        public MainServer(ServerConfig config)
        {
            listener = new TcpListener(IPAddress.Any, config.port);
            format = config.format;
            color = config.color;

            listener.Start();

            listenthread = new Thread(new ThreadStart(Listening));
            listenthread.Start();
        }

        public void SendMsg(Source sender, string message)
        {
            var msg = FitReply(format, message, sender);

            lock (clients)
                foreach (var client in clients)
                    client.SendMsg(msg, color);
        }

        public void Dispose()
        {
            listener.Stop();
            lock (clients)
                foreach (var client in clients)
                    client.Dispose();
        }
    }
}
