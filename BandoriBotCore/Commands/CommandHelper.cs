using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public sealed class AdminAttribute : Attribute
    {

    }

    public static class CommandHelper
    {
        private const string noperm = "没有权限";

        private static bool ParseCommand(Type t, CommandArgs arg, List<string> args)
        {
            if (args.Count > 0)
            {
                foreach (var type in t.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
                {
                    if (args[0] == type.Name.ToLower() && ParseCommand(type, arg, args.Skip(1).ToList()))
                    {
                        return true;
                    }
                }
            }

            foreach (var method in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
            {
                var lst = args;
                if (method.Name != "Main")
                {
                    if (args.FirstOrDefault() != method.Name.ToLower())
                        continue;
                    lst = args.Skip(1).ToList();
                }

                List<object> objs = new List<object>();

                bool error = false;
                var index = 0;

                foreach (var param in method.GetParameters())
                {
                    if (objs.Count == 0)
                    {
                        if (param.ParameterType != typeof(CommandArgs))
                        {
                            error = true;
                            break;
                        }
                        objs.Add(arg);
                        continue;
                    }

                    try
                    {
                        if (param.ParameterType == typeof(string))
                            objs.Add(lst[index++]);
                        else
                            objs.Add(param.ParameterType.GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)
                                .Invoke(null, new object[] { lst[index++] }));
                    }
                    catch
                    {
                        error = true;
                        break;
                    }
                }

                if (error || index != lst.Count) continue;

                if (method.IsDefined(typeof(AdminAttribute)) && !arg.Source.CheckPermission().Result)
                {
                    arg.Callback(noperm);
                    return true;
                }


                method.Invoke(null, objs.ToArray());
                return true;
            }
            return false;
        }

        public static void Register<T>(string name = null, string prefix = "#")
        {
            MessageHandler.Register(new LegacyCommand(prefix + (name ?? typeof(T).Name.ToLower()),
                (args) => ParseCommand(typeof(T), args, args.Arg.Split(' ').Where((s) => !string.IsNullOrWhiteSpace(s)).ToList())));
        }
    }
}
