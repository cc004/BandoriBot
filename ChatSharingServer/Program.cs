using System;
using BandoriBot.Terraria;

namespace ChatSharingServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = ushort.Parse(args[0]);
            var svr = new MainServer(port);
            svr.ServeForever();
        }
    }
}
