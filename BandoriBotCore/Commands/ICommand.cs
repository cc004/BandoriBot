using BandoriBot.Handler;
using BandoriBot.Models;
using Mirai_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BandoriBot.Commands
{
    public struct CommandArgs
    {
        public Action<string> Callback;
        public string Arg;
        public bool IsAdmin;
        public Source Source;
    }

    public interface ICommand
    {
        List<string> Alias { get; }

        void Run(CommandArgs args);
    }
}
