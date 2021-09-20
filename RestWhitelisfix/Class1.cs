using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TShockAPI;
using Terraria;
using TerrariaApi.Server;
using Microsoft.Xna.Framework;
using System.IO;

namespace RestWhitelistfix
{
    [ApiVersion(2, 1)]
    public class RestWhitelistfix : TerrariaPlugin
    {
        /// <summary>
        /// Gets the author(s) of this plugin
        /// </summary>
        public override string Author => "Leader";

        /// <summary>
        /// Gets the description of this plugin.
        /// A short, one lined description that tells people what your plugin does.
        /// </summary>
        public override string Description => "A sample plugin to fix rest whitelist mannager.";

        /// <summary>
        /// Gets the name of this plugin.
        /// </summary>
        public override string Name => "RestWhitelistFix";

        /// <summary>
        /// Gets the version of this plugin.
        /// </summary>
        public override Version Version => new Version(1, 0, 0, 0);

        /// <summary>
        /// Initializes a new instance of the RestWhitelistfix class.
        /// This is where you set the plugin's order and perfrom other constructor logic
        /// </summary>
        public RestWhitelistfix(Main game) : base(game)
        {

        }

        /// <summary>
        /// Handles plugin initialization. 
        /// Fired when the server is started and the plugin is being loaded.
        /// You may register hooks, perform loading procedures etc here.
        /// </summary>
        public override void Initialize()
        {
            Commands.ChatCommands.Add(new Command("wl.admin", wl, "wl"));
        }

        private void wl(CommandArgs args)
        {
            string name = args.Parameters[0];
            bool enable = bool.Parse(args.Parameters[1]);
            var lists = new List<string>();
            using (StreamReader re=new StreamReader("tshock/whitelist.txt"))
            {
                while (true)
                {
                    var s = re.ReadLine();
                    if (s == null)
                        break;
                    if (s != "")
                        lists.Add(s);
                }
            }
            if(enable)
            {
                foreach(var s in lists)
                {
                    if (s == name)
                        return;
                }
                lists.Add(name);
            }
            else
            {
                for(int i = 0; i < lists.Count; i++)
                {
                    if(lists[i]==name)
                    {
                        lists.RemoveAt(i);
                    }
                }
            }
            using (StreamWriter wr=new StreamWriter("tshock/whitelist.txt"))
            {
                foreach (var s in lists)
                {
                    wr.WriteLine(s);
                }
            }
        }

        /// <summary>
        /// Handles plugin disposal logic.
        /// *Supposed* to fire when the server shuts down.
        /// You should deregister hooks and free all resources here.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Deregister hooks here
            }
            base.Dispose(disposing);
        }
    }
}