using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public struct CommandArgs
    {
        public Func<string, Task> Callback;
        public string Arg;
        public Source Source;
    }

    public interface ICommand
    {
        List<string> Alias { get; }

        string Permission => null;
        Task Run(CommandArgs args);
    }
}
