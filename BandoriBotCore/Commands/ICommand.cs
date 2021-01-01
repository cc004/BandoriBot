using BandoriBot.Handler;
using BandoriBot.Models;
using Mirai_CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public struct CommandArgs
    {
        public Func<string, Task> Callback;
        public string Arg;
        public bool IsAdmin;
        public Source Source;
    }

    public interface ICommand
    {
        List<string> Alias { get; }

        Task Run(CommandArgs args);
    }
}
