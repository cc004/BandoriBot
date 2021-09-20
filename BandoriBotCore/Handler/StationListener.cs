using BandoriBot.DataStructures;
using BandoriBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    public class StationListener
    {

        private const string uri = "wss://api.bandoristation.com";
        private ClientWebSocket client;
        private List<Car> cars;
        public bool Running { get; private set; }
        public bool Active { get; private set; }

        public event Action<Car> OnNewCar;

        public List<Car> Cars
        {
            get
            {
                var t = DateTime.Now;

                lock (cars)
                {
                    cars = cars.Where(c =>
                    {
                        if (t - c.time > new TimeSpan(0, 2, 0)) return false;
                        return true;
                    }).Reverse().ToList();
                    return cars;
                }
            }
        }

        public StationListener()
        {
            Active = false;
            cars = new List<Car>();
            Running = false;
        }

        private void StationListener_OnMsg(JObject obj)
        {
            if (obj["status"].ToString() != "success" || obj["action"].ToString() != "sendRoomNumberList") return;

            foreach (JObject car in (JArray)obj["response"])
            {
                var c = new Car(car);
                lock (cars)
                    cars.Add(c);
                OnNewCar?.Invoke(c);
            }
        }

        private async Task SendMsg(object json)
        {
            if (client.State == WebSocketState.Open)
                await client.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(json.ToString())), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task Login()
        {
            await SendMsg(new JObject
            {
                ["action"] = "setClient",
                ["data"] = new JObject
                {
                    ["send_room_number"] = true,
                    ["send_chat"] = false
                }
            });
        }

        private async Task Connect()
        {
            while (Running && (client == null || client.State != WebSocketState.Open))
            {
                try
                {
                    client = new ClientWebSocket();
                    using (Task connect = client.ConnectAsync(new Uri(uri), CancellationToken.None))
                        if (await Task.WhenAny(connect, Task.Delay(5000)) != connect)
                            this.Log(LoggerLevel.Warn, "Exception normal : websocket connection timed out.");

                    await Login();

                    this.Log(LoggerLevel.Info, "Connected to Bandori Station");
                }
                catch { }
            }
            Active = Running;
        }

        public async Task Stop()
        {
            Running = false;
            await client.CloseAsync(WebSocketCloseStatus.Empty, "connection closed.", CancellationToken.None);
        }

        public void Start()
        {
            Running = true;
            Connect().Wait();

            Task.Run(async () =>
            {
                while (Running)
                {
                    try
                    {
                        await SendMsg(new JObject
                        {
                            ["action"] = "heartbeat",
                            ["data"] = new JObject
                            {
                                ["client"] = "喵喵喵"
                            }
                        });

                    }
                    catch { }
                    Thread.Sleep(10000);
                }
            });

            Task.Run(async () =>
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
                                        if (Running) await Connect();
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

                            StationListener_OnMsg(JObject.Parse(Encoding.UTF8.GetString(ms.ToArray())));
                        }
                    }
                    catch (Exception e)
                    {
                        this.Log(LoggerLevel.Warn, "Uncaught exception : " + e.ToString());
                    }
                }
            });
        }

    }
}
