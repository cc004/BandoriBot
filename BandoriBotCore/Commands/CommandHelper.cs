using BandoriBot.Handler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class PermissionAttribute : Attribute
    {
        public string permission;
        public PermissionAttribute(string permission)
        {
            this.permission = permission;
        }
    }

    public sealed class SuperadminAttribute : Attribute
    {

    }

    public static class CommandHelper
    {
        private const string noperm = "没有权限";

        private static async Task<bool> ParseCommand(Type t, CommandArgs arg, List<string> args)
        {
            if (args.Count > 0)
            {
                foreach (var type in t.GetNestedTypes(BindingFlags.Public | BindingFlags.Static))
                {
                    if (args[0] == type.Name.ToLower() && await ParseCommand(type, arg, args.Skip(1).ToList()))
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

                if (method.IsDefined(typeof(SuperadminAttribute)) && !await arg.Source.HasPermission("*") ||
                    method.IsDefined(typeof(PermissionAttribute)) && !await arg.Source.HasPermission(method.GetCustomAttribute<PermissionAttribute>().permission))
                {
                    await arg.Callback(noperm);
                    return true;
                }


                method.Invoke(null, objs.ToArray());
                return true;
            }

            var method2 = t.GetMethod("Default", BindingFlags.Public | BindingFlags.Static);
            if (method2 != null)
            {
                if (method2.IsDefined(typeof(SuperadminAttribute)) && !await arg.Source.HasPermission("*") ||
                    method2.IsDefined(typeof(PermissionAttribute)) && !await arg.Source.HasPermission(method2.GetCustomAttribute<PermissionAttribute>().permission))
                {
                    await arg.Callback(noperm);
                    return true;
                }

                method2.Invoke(null, new object[] { arg });
                return true;
            }
            return false;
        }

        private static readonly string prefix = File.Exists("prefix.txt") ? File.ReadAllText("prefix.txt") : "";
        public static void Register<T>(string name = null, string prefix = null)
        {
            if (prefix == null)
                prefix = CommandHelper.prefix;
            MessageHandler.Register(new LegacyCommand(prefix + (name ?? typeof(T).Name.ToLower()),
                (args) => ParseCommand(typeof(T), args, args.Arg.Split(' ').Where((s) => !string.IsNullOrWhiteSpace(s)).ToList())));
        }
    }
}
