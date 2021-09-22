
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;

namespace BandoriBot.Terraria
{
    public sealed class MainServer : IDisposable
    {
        private TcpListener listener;
        private readonly List<GameServer> clients = new ();
        public void ServeForever()
        {
            Console.WriteLine("main server is accepting");
            while (true)
            {
                try
                {
                    var client = listener.AcceptTcpClient();
                    Console.WriteLine($"accepted socket from {client.Client.RemoteEndPoint}");
                    var svr = new GameServer(client);
                    svr.OnMessage += (msg, clr) =>
                    {
                        Console.WriteLine($"[{svr.Name}|{clr:X6}] {msg}");
                        lock (clients)
                            foreach (var client in clients.Where(client => client != svr))
                                client.SendMsg(msg, clr);
                    };

                    lock (clients)
                    {
                        clients.RemoveAll(s =>
                        {
                            if (s.Valid) return false;
                            s.Dispose();
                            return true;
                        });
                        clients.Add(svr);
                    }
                }
                catch
                {

                }

            }
        }

        public MainServer(ushort port)
        {
            listener = new TcpListener(IPAddress.Any, port);

            listener.Start();
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
