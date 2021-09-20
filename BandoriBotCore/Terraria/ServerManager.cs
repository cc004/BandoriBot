
using System;
using System.Collections.Generic;
using System.IO;
using BandoriBot;
using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Models;
using Newtonsoft.Json.Linq;

namespace Native.Csharp.App.Terraria
{
    public class ServerManager : JsonConfiguration
    {
        public Dictionary<string, Server> servers;

        public Dictionary<long, string> bindings = new Dictionary<long, string>();

        public Server GetServer(CommandArgs args)
        {
            if (!bindings.ContainsKey(args.Source.FromQQ))
                //throw new CommandException("您未设定客户端类型，您可以输入:泰拉切换 (泰拉PE/泰拉PC)改洪荒1 修改客户端类型，您可以随时进行修改。请指定好客户端重发指令哦~");
                throw new CommandException("您未设定服务器类型，您可以输入:泰拉切换 流光之城 修改服务端类型，您可以随时进行修改。请指定好客户端重发指令哦~");
            return servers[bindings[args.Source.FromQQ]];
        }

        public string GetServerName(CommandArgs args)
        {
            if (!bindings.ContainsKey(args.Source.FromQQ))
                //throw new CommandException("您未设定客户端类型，您可以输入:泰拉切换 (泰拉PE/泰拉PC)改洪荒1 修改客户端类型，您可以随时进行修改。请指定好客户端重发指令哦~");
                throw new CommandException("您未设定服务器类型，您可以输入:泰拉切换 流光之城 修改服务端类型，您可以随时进行修改。请指定好客户端重发指令哦");
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
                    var svr = new Server((string)obj.Value["endpoint"], (int)obj.Value["group"])
                    {
                        display = obj.Value.Value<bool>("display"),
                        noRegister = obj.Value.Value<string>("noRegister")
                    };
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
