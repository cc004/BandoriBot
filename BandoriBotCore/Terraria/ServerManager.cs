using BandoriBot.Config;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BandoriBot;
using BandoriBot.Models;
using BandoriBot.Commands;

namespace Native.Csharp.App.Terraria
{
    public class ServerManager : JsonConfiguration
    {
        public Dictionary<string, Server> servers;

        public Dictionary<long, string> bindings = new Dictionary<long, string>();

        public Server GetServer(CommandArgs args)
        {
            if (!bindings.ContainsKey(args.Source.FromQQ))
                throw new CommandException("尚未切换到任意服务器");
            return servers[bindings[args.Source.FromQQ]];
        }

        public string GetServerName(CommandArgs args)
        {
            if (!bindings.ContainsKey(args.Source.FromQQ))
                throw new CommandException("尚未切换到任意服务器");
            return bindings[args.Source.FromQQ];
        }

        public void SwitchTo(long qq, string name)
        {
            if (!bindings.ContainsKey(qq))
                bindings.Add(qq, null);

            bindings[qq] = name;
        }

        public override string Name => "rest.json";

        public override void LoadDefault()
        {
            servers = new Dictionary<string, Server>();
        }
        public override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            servers = new Dictionary<string, Server>();

            foreach (var obj in (json as JObject).Properties())
            {
                try
                {
                    var svr = new Server((string)obj.Value["endpoint"], (int)obj.Value["group"]);
                    svr.Login((string)obj.Value["username"], (string)obj.Value["password"]);
                    svr.RunCommand("/csreload");
                    servers.Add(obj.Name, svr);
                }
                catch (Exception e)
                {
                    Utils.Log(LoggerLevel.Error, obj.ToString() + '\n' + e.ToString());
                }
            }
        }
    }
}
