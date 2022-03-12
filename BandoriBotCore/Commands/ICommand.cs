using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class CommandArgs
    {
        public Func<string, Task> Callback;
        public string Arg, Trigger;
        public Source Source;
        private readonly HandlerArgs parent;

        public Task finishedTask
        {
            get => parent.finishedTask;
            set => parent.finishedTask = value;
        }

        public CommandArgs(HandlerArgs args)
        {
            parent = args;
        }
    }

    public interface ICommand
    {
        List<string> Alias { get; }

        string Permission => null;
        Task Run(CommandArgs args);
    }
}
