using BandoriBot.Handler;
using BandoriBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BandoriBot.Commands
{
    public struct CommandArgs
    {
        public ResponseCallback Callback;
        public string Arg;
        public bool IsAdmin;
        public Source Source;
    }

    public abstract class Command : IMessageHandler
    {
        protected abstract List<string> Alias { get; }

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            string lower = message.ToLower();
            foreach (string alias in Alias)
            {
                if (lower.StartsWith(alias))
                {
                    try
                    {
                        Run(new CommandArgs()
                        {
                            Arg = message.Substring(alias.Length),
                            Callback = callback,
                            IsAdmin = isAdmin,
                            Source = Sender
                        });
                    }
                    catch (Exception e)
                    {
                        callback($"Unhandled exception : {e}");
                    }
                    return true;
                }
            }
            return false;
        }

        protected abstract void Run(CommandArgs args);
    }
}
