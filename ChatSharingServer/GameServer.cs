
using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace BandoriBot.Terraria
{
    public sealed class GameServer : IDisposable
    {
        private enum MsgType
        {
            SetServerName,
            WriteMessage,
            Heartbeat
        }
        private TcpClient client;
        private NetworkStream stream;
        private BinaryReader br;
        private BinaryWriter bw;
        private Thread recvthread, tickthread;

        public string Name { get; set; }
        public bool Valid { get; private set; }

        public event Action<string, uint> OnMessage;
        public event Action OnClose;

        private void Listener()
        {
            try
            {
                var buf = new byte[4];
                while (Valid)
                {
                    Thread.Sleep(0);
                    stream.Read(buf, 0, 4);
                    switch ((MsgType)BitConverter.ToInt32(buf, 0))
                    {
                        case MsgType.SetServerName:
                            Name = br.ReadString();
                            Console.WriteLine($"set server = {Name}");
                            break;
                        case MsgType.WriteMessage:
                            var msg = br.ReadString();
                            var clr = br.ReadUInt32();
                            OnMessage?.Invoke(msg, clr);
                            break;
                    }
                }
            }
            catch
            {
                try
                {
                    Close();
                }
                catch
                {

                }
            }
        }

        private void Close()
        {
            Valid = false;
            client.Close();
        }
        private void Heartbeat()
        {
            try
            {
                while (Valid)
                {
                    Thread.Sleep(1000);
                    lock (bw)
                        bw.Write((int)MsgType.Heartbeat);
                }
            }
            catch
            {
                try
                {
                    Close();
                }
                catch
                {

                }
            }
        }

        public GameServer(TcpClient client)
        {
            this.client = client;
            stream = client.GetStream();
            br = new BinaryReader(stream);
            bw = new BinaryWriter(stream);
            Valid = true;
            recvthread = new Thread(Listener);
            tickthread = new Thread(Heartbeat);
            recvthread.Start();
            tickthread.Start();
        }

        public void SetName(string name)
        {
            try
            {
                bw.Write((int)MsgType.SetServerName);
                bw.Write(name);
            }
            catch
            {

            }

        }

        public void SendMsg(string message, uint color = 0xffffff)
        {
            try
            {
                if (string.IsNullOrEmpty(message)) return;

                lock (bw)
                {
                    bw.Write((int)MsgType.WriteMessage);
                    bw.Write(message);
                    bw.Write(color);
                }
            }
            catch
            {

            }
        }

        public void Dispose()
        {
            OnClose?.Invoke();
            br.Dispose();
            bw.Dispose();
            try
            {
                client.Close();
            }
            catch
            {

            }
            Valid = false;
        }
    }

}
