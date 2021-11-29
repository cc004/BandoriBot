using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BandoriBot.Config
{
    internal class InteractClient
    {
        public event Action<string> Input;

        public void Output(string msg)
        {
            writer?.Write(msg);
            writer?.Flush();
        }

        private TcpClient client;
        private BinaryReader reader;
        private BinaryWriter writer;
        private IPEndPoint ep;

        public Queue<string> history = new();

        public InteractClient(string host)
        {
            var s = host.Split(':');
            ep = new(IPAddress.Parse(s[0]), int.Parse(s[1]));

            new Thread(Listen).Start();
            new Thread(() =>
            {
                for (; ; )
                {
                    try
                    {
                        writer.Write("");
                    }
                    catch
                    {

                    }

                    Thread.Sleep(1000);
                }
            }).Start();

        }

        private void Listen()
        {
            while (true)
            {
                try
                {
                    client = new TcpClient();
                    client.Connect(ep);
                    reader = new BinaryReader(client.GetStream());
                    writer = new BinaryWriter(client.GetStream());
                    for (; ; )
                    {
                        var s = reader.ReadString();
                        if (!string.IsNullOrEmpty(s))
                        {
                            history.Enqueue(s);
                            if (history.Count > 1000) history.Dequeue();
                            Input?.Invoke(s);
                        }
                    }
                }
                catch
                {

                }
            }
        }
    }
    public class Pipe : SerializableConfiguration<Dictionary<long, string>>
    {
        public override string Name => "pipe.json";
        private Dictionary<long, InteractClient> interacts = new();

        public override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            interacts = t.ToDictionary(pair => pair.Key, pair => new InteractClient(pair.Value));

            foreach (var pair in interacts)
                pair.Value.Input += msg =>
                {
                    this.Log(Models.LoggerLevel.Debug, msg);
                    //MessageHandler.session.SendGroupMessage(pair.Key, CQCode.CQText(msg)).AsTask().Wait();
                };
        }

        public string GetHistory(long group)
        {
            return string.Join('\n', interacts[group].history);
        }

        public void SendMsg(long group, string msg)
        {
            if (interacts.TryGetValue(group, out var val))
                val.Output(msg);
        }

        public override void LoadDefault()
        {
            t = new();
        }
    }
}
