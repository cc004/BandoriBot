/*
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
                            this.Log(Models.LoggerLevel.Info, $"set server = {Name}");
                            break;
                        case MsgType.WriteMessage:
                            var msg = br.ReadString();
                            var clr = br.ReadUInt32();
                            this.Log(Models.LoggerLevel.Debug, $"recv msg {msg}");
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
            recvthread = new Thread(new ThreadStart(Listener));
            tickthread = new Thread(new ThreadStart(Heartbeat));
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
*/