using BandoriBot.Handler;
using BandoriBot.Models;
using Mirai_CSharp;
using Mirai_CSharp.Models;
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
        public Source Source;
    }

    public class PermissionAttribute : Attribute
    {
        public GroupPermission permission;
        public PermissionAttribute(GroupPermission permission)
        {
            this.permission = permission;
        }
    }

    public interface ICommand
    {
        List<string> Alias { get; }

        Task Run(CommandArgs args);
    }
}
