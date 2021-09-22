
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using BandoriBot.Config;
using BandoriBot.Handler;
using Sora.Enumeration.EventParamsType;

namespace BandoriBot.Terraria
{

    public sealed class MainServer : IDisposable
    {
        private GameServer server;
        private static Regex replace = new Regex(@"\$((?!&).|&...;)", RegexOptions.Compiled);
        private ServerConfig config;

        public event Action<string> OnServerMessage;
        private static string FitReply(string format, string message, Source sender)
            => replace.Replace(format, m =>
            {
                var c = m.Value[1];
                return c switch
                {
                    'm' => message,
                    '$' => "$",
                    'g' => sender.FromGroup.ToString(),
                    'q' => sender.FromQQ.ToString(),
                    'c' => sender.GetName().Result,
                    _ => m.Value
                };
            });
        

        public MainServer(ServerConfig config)
        {
            this.config = config;
            server = new GameServer(config.host, config.port, "bot");
            server.OnMessage += Server_OnMessage;
        }

        private void Server_OnMessage(string message, uint color)
        {
            OnServerMessage?.Invoke(message);
        }

        public void SendMsg(Source sender, string message)
        {
            var msg = FitReply(config.format, message, sender);

            var color = (sender.GetRole().Result) switch
            {
                MemberRoleType.Owner => config.owner_color,
                MemberRoleType.Admin => config.admin_color,
                _ => config.member_color
            };

            server.SendMsg(msg, color);
        }

        public void Dispose()
        {
            server.Dispose();
        }
    }
}
