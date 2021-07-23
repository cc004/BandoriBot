using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    [Serializable]
    public class CommandException : Exception{
        public CommandException(string message) : base(message)
        {
        }

        public CommandException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CommandException()
        {
        }

        protected CommandException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
        {
        }
    }
    public class LegacyCommand : ICommand
    {
        private string Command;
        private Func<CommandArgs, Task> Action;

        public LegacyCommand(string Command, Func<CommandArgs, Task> Action)
        {
            this.Command = Command;
            this.Action = Action;
        }

        public List<string> Alias => new List<string> { Command };

        public async Task Run(CommandArgs args)
        {
            try
            {
                await Action(args);
            }
            
            catch (TargetInvocationException e)
            {
                if (e.InnerException is CommandException e2)
                    await args.Callback(e2.Message);
                else
                    throw;
            }
            catch (CommandException e)
            {
                await args.Callback(e.Message);
            }
        }
    }
}
