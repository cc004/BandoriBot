using BandoriBot.Handler;
using BandoriBot.Terraria;
using Mirai_CSharp.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot.Config
{
    [JsonObject]
    public class ServerConfig
    {
        public int port;
        public string format = string.Empty;
        public long[] groups = Array.Empty<long>();
        public uint color;
    }

    public class MainServerConfig : SerializableConfiguration<ServerConfig>, IMessageHandler
    {
        public override string Name => "trserver.json";
        private MainServer server;

        public override void LoadDefault()
        {
            t = new ServerConfig();
        }

        public override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            server = new MainServer(t);
            server.OnServerMessage += msg =>
            {
                foreach (var group in t.groups)
                    MessageHandler.session.SendGroupMessageAsync(group, new PlainMessage(msg)).Wait();
            };
        }

        public void SendMsg(string msg, Source sender)
        {
            server.SendMsg(sender, msg);
        }

        public override void Dispose()
        {
            server.Dispose();
        }
        public bool IgnoreCommandHandled => true;

        public async Task<bool> OnMessage(HandlerArgs args)
        {
            if (t.groups.Contains(args.Sender.FromGroup))
                SendMsg(args.message, args.Sender);

            return false;
        }
    }
}
