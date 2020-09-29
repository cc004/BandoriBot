using BandoriBot.Config;
using BandoriBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BandoriBot.Commands
{
    public abstract class Activator : ICommand
    {
        private static string GetTrans(bool status)
        {
            return status ? "开启" : "关闭";
        }
        public abstract List<string> Alias { get; }
        protected abstract bool Status { get; }
        public void Run(CommandArgs args)
        {
            args.Arg = args.Arg.Trim();
            if (args.Arg.Length == 0)
            {
                Configuration.GetConfig<Activation>()[args.Source.FromQQ] = Status;
                args.Callback($"个人车牌转发已{GetTrans(Status)}");
            }
            else
            {
                long group;
                try
                {
                    if (args.Arg == "~" && args.Source.FromGroup > 0)
                        group = args.Source.FromGroup;
                    else
                    {
                        List<GroupMemberInfo> source;
                        group = long.Parse(args.Arg);
                        if (!args.IsAdmin)
                        {
                            source = Common.CqApi.GetMemberList(group).Where((GroupMemberInfo info) => (info.QQId == args.Source.FromQQ)).ToList();
                            if (source.Count == 0 || source[0].PermitType == PermitType.None)
                            {
                                args.Callback("权限不足");
                                return;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    args.Callback("群号错误");
                    return;
                }
                Configuration.GetConfig<Activation>()[group] = Status;
                args.Callback($"{group}的车牌转发已{GetTrans(Status)}");
            }
        }

    }
    public class Activate : Activator
    {
        protected override bool Status => true;

        public override List<string> Alias => new List<string>
        {
            "/activate"
        };
    }
    public class Deactivate : Activator
    {
        protected override bool Status => false;

        public override List<string> Alias => new List<string>
        {
            "/deactivate"
        };
    }
}
