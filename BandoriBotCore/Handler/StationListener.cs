using BandoriBot.DataStructures;
using BandoriBot.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public class StationListener
    {
        private const string uri = "wss://api.bandoristation.com:50443";
        private ClientWebSocket client;
        private List<Car> cars;
        public bool Running { get; private set; }
        public bool Active { get; private set; }
        public List<Car> Cars
        {
            get
            {
                var t = DateTime.Now;

                lock (cars)
                {
                    return cars.Where(c =>
                        {
                            if (t - c.time > new TimeSpan(0, 2, 0)) return false;
                            return true;
                        }).Reverse().ToList();
                }
            }
        }

        public StationListener()
        {
            Active = false;
            cars = new List<Car>();
            Running = false;
        }

        private void Connect()
        {
            while (Running && (client == null || client.State != WebSocketState.Open))
            {
                try
                {
                    client = new ClientWebSocket();
                    using (Task connect = client.ConnectAsync(new Uri(uri), CancellationToken.None))
                        if (Task.WhenAny(connect, Task.Delay(5000)).Result != connect)
                            this.Log(LoggerLevel.Info, "Exception normal : websocket connection timed out.");
                }
                catch { }
            }
            Active = Running;
        }
        public void Stop()
        {
            Running = false;
            client.CloseAsync(WebSocketCloseStatus.Empty, "connection closed.", CancellationToken.None).Wait();
        }
        public void Start()
        {
            Running = true;
            Connect();
            new Thread(new ThreadStart(delegate ()
            {
                ArraySegment<byte> heartbeat = new ArraySegment<byte>(Encoding.UTF8.GetBytes("heartbeat"));
                while (Running)
                {
                    try
                    {
                        if (client.State == WebSocketState.Open)
                            client.SendAsync(heartbeat, WebSocketMessageType.Text, true, CancellationToken.None).Wait();
                    }
                    catch { }
                    Thread.Sleep(10000);
                }
            })).Start();
            new Thread(new ThreadStart(delegate ()
            {
                byte[] buffer = new byte[1 << 16];
                while (Running)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            WebSocketReceiveResult result = null;
                            while (true)
                            {
                                while (result == null)
                                {
                                    try
                                    {
                                        result = client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None).Result;
                                    }
                                    catch (AggregateException)
                                    {
                                        Active = false;
                                        if (Running) Connect();
                                    }
                                }

                                if (result.Count == 0)
                                {
                                    result = null;
                                    continue;
                                }

                                ms.Write(buffer, 0, result.Count);
                                if (result.EndOfMessage) break;
                            }

                            try
                            {
                                /*
                                using var client = new TcpClient();
                                client.Connect("39.106.92.32", 7777);
                                using var br = new BinaryReader(client.GetStream());
                                using var bw = new BinaryWriter(client.GetStream());*/

                                JObject obj = JObject.Parse(Encoding.UTF8.GetString(ms.ToArray()));
                                if (obj["status"].ToString() != "success") continue;
                                foreach (JObject car in (JArray)obj["response"])
                                {
                                    var c = new Car(car);
                                    /*
                                    bw.Write(c.index);
                                    var s = br.ReadString();
                                    if (string.IsNullOrEmpty(s)) continue;
                                    var j = JObject.Parse(s);
                                    if ((int)j["num"] == 5) continue;

                                    var type = (string)j["property"];
                                    c.rawmessage = (type == "private_100_Normal_2.9" ? "18w" :
                                                    type == "private_10_Normal_2.9" ? "12w" :
                                                    type == "private_2_Normal_2.9" ? "7w" : "0w") +
                                                    $"q{5 - (int)j["num"]} " + c.rawmessage
                                        .Replace("q1", "")
                                        .Replace("q2", "")
                                        .Replace("q3", "")
                                        .Replace("q4", "")
                                        .Replace("18w", "")
                                        .Replace("7w", "")
                                        .Replace("12w", "")
                                        .Replace("自由", "")
                                        .Replace("大师", "");
                                        */
                                    lock (cars)
                                        cars.Add(c);
                                }
                            }
                            catch (JsonReaderException e)
                            {
                                this.Log(LoggerLevel.Warn, e.ToString());
                                this.Log(LoggerLevel.Warn, "when trying to decode : " + Encoding.UTF8.GetString(ms.ToArray()));
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        this.Log(LoggerLevel.Warn, "Uncaught exception : " + e.ToString());
                    }
                }
            })).Start();
        }

    }
}
