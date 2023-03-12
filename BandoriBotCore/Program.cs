using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Services;
using Sora.EventArgs.SoraEvent;
using Sora.Net;
using Sora.OnebotModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BandoriBot.Models;
using Sora;
using Sora.Entities.Base;
using Sora.Enumeration.EventParamsType;
using Sora.Net.Config;
using YukariToolBox.LightLog;

namespace BandoriBot
{
    class Program
    {
        private static void PluginInitialize()
        {
            if (!Directory.Exists("Plugins")) Directory.CreateDirectory("Plugins");
            foreach (var type in new []{Assembly.GetExecutingAssembly()}
                         .Concat(Directory.GetFiles("Plugins").Select(file => Assembly.LoadFrom(file)))
                         .SelectMany(asm => asm.GetTypes()))
            {
                if (type.IsAbstract) continue;

                object o = null;
                if (type.IsAssignableTo(typeof(Configuration)))
                {
                    Configuration.Register((Configuration)(o ??= Activator.CreateInstance(type)));
                    Utils.Log(LoggerLevel.Info, $"registering {o} to configuration");
                }

                if (type.IsAssignableTo(typeof(ICommand)))
                {
                    MessageHandler.Register((ICommand)(o ??= Activator.CreateInstance(type)));
                    Utils.Log(LoggerLevel.Info, $"registering {o} to command");
                }

                if (type.IsAssignableTo(typeof(IMessageHandler)))
                {
                    MessageHandler.Register((IMessageHandler)(o ??= Activator.CreateInstance(type)));
                    Utils.Log(LoggerLevel.Info, $"registering {o} to handler");
                }
            }
            RecordDatabaseManager.InitDatabase();
            Configuration.LoadAll();
            MessageHandler.SortHandler();
            GC.Collect();

        }

        private static async Task Testing()
        {
            var client = SekaiClient.SekaiClient.GetClient();
        }

        public static void Main(string[] args)
        {
            Log.SetLogLevel(LogLevel.Fatal);
            //Testing().Wait();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            
            new Thread(() => Apis.Program.Main2(args)).Start();
            var tasks = new List<Task>();

            PluginInitialize();

            foreach (var line in File.ReadAllLines("cqservers.txt"))
            {
                var s = line.Split(":");
                var service = SoraServiceFactory.CreateService(new ClientConfig()
                {
                    Host = s[0],
                    Port = ushort.Parse(s[1])
                });

                service.Event.OnClientConnect += Event_OnClientConnect;
                service.Event.OnFriendRequest += Event_OnFriendRequest;
                service.Event.OnGroupMessage += Event_OnGroupMessage;
                service.Event.OnPrivateMessage += Event_OnPrivateMessage;
                service.Event.OnGroupRequest += Event_OnGroupRequest;
                service.Event.OnGuildMessage += Event_OnGuildMessage;

                Console.WriteLine("connected to server");

                tasks.Add(service.StartService().AsTask());
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var root = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                return Assembly.LoadFrom(Path.Combine(root, $"{new AssemblyName(args.Name).Name}.dll"));
            }
            catch
            {
                return null;
            }
        }

        private static async ValueTask Event_OnGuildMessage(string eventType, GuildMessageEventArgs eventArgs)
        {
            await MessageHandler.OnMessage(eventArgs.SoraApi, Utils.GetCQMessage(eventArgs.Message), new Source
            {
                Session = eventArgs.SoraApi,
                FromGroup = MessageHandler.HashGroupCache(eventArgs.Guild, eventArgs.Channel),
                IsGuild = true,
                FromQQ = eventArgs.SenderInfo.UserId,
                time = eventArgs.Time
            });
        }

        private static async ValueTask Event_OnGroupRequest(string type, AddGroupRequestEventArgs eventArgs)
        {
            if (eventArgs.SubType == GroupRequestType.Invite)
                await eventArgs.Accept();
        }

        private static async ValueTask Event_OnPrivateMessage(string type, PrivateMessageEventArgs eventArgs)
        {
            await MessageHandler.OnMessage(eventArgs.SoraApi, Utils.GetCQMessage(eventArgs.Message), new Source
            {
                Session = eventArgs.SoraApi,
                FromGroup = 0,
                FromQQ = eventArgs.SenderInfo.UserId,
                time = eventArgs.Time
            });
        }

        private static async ValueTask Event_OnGroupMessage(string type, GroupMessageEventArgs eventArgs)
        {
            await MessageHandler.OnMessage(eventArgs.SoraApi, Utils.GetCQMessage(eventArgs.Message), new Source
            {
                Session = eventArgs.SoraApi,
                FromGroup = eventArgs.SourceGroup.Id,
                FromQQ = eventArgs.SenderInfo.UserId,
                time = eventArgs.Time
            });
        }

        private static async ValueTask Event_OnFriendRequest(string type, FriendRequestEventArgs eventArgs)
        {
            await eventArgs.SoraApi.SetFriendAddRequest(eventArgs.RequestFlag, true);
        }

        private static async ValueTask Event_OnClientConnect(string type, Sora.EventArgs.SoraEvent.ConnectEventArgs eventArgs)
        {
            lock (MessageHandler.bots)
            {
                if (MessageHandler.bots.ContainsKey(eventArgs.LoginUid))
                    MessageHandler.bots.Remove(eventArgs.LoginUid);
                MessageHandler.bots.Add(eventArgs.LoginUid, eventArgs.SoraApi);
                MessageHandler.selfids.Add(eventArgs.LoginUid);
            }
            MessageHandler.booted = true;
        }

        private static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            var ex = e.Exception;
            //while (ex is AggregateException e2) ex = e2.InnerException;
            //if (ex is ApiException) return;
            //if (ex is IOException) return;

            Console.WriteLine(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RecordDatabaseManager.Close();
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
