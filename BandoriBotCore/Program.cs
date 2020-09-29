using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Services;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Native.Csharp.App.Terraria;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot
{
    class Program
    {
        private static void PluginInitialize(MiraiHttpSession session)
        {
            Configuration.Register(new Activation());
            Configuration.Register(new Delay());
            Configuration.Register(new MessageStatistic());
            Configuration.Register(new ReplyHandler());
            Configuration.Register(new Whitelist());
            Configuration.Register(new Admin());
            Configuration.Register(new Blacklist());
            Configuration.Register(new TitleCooldown());
            Configuration.Register(new PCRConfig());
            Configuration.Register(new R18Allowed());
            Configuration.Register(new NormalAllowed());
            Configuration.Register(new AccountBinding());
            Configuration.Register(new ServerManager());
            Configuration.Register(new TimeConfiguration());
            Configuration.Register(new GlobalConfiguration());
            Configuration.Register(new Antirevoke());
            Configuration.Register(new SetuConfig());
            Configuration.Register(new Save());
            //Configuration.Register(new PeriodRank());
            Configuration.LoadAll();

            MessageHandler.Register(new WhitelistHandler());
            MessageHandler.Register(Configuration.GetConfig<MessageStatistic>());
            MessageHandler.Register(new YCM());
            MessageHandler.Register(new CarHandler());
            MessageHandler.Register(new QueryCommand());
            MessageHandler.Register(new ReplyCommand());
            MessageHandler.Register(new FindCommand());
            MessageHandler.Register(new DelayCommand());
            MessageHandler.Register(new AdminCommand());
            MessageHandler.Register(new WhitelistCommand());
            MessageHandler.Register(new GachaCommand());
            MessageHandler.Register(new GachaListCommand());
            MessageHandler.Register(new Activate());
            MessageHandler.Register(new Deactivate());
            MessageHandler.Register(new BlacklistCommand());
            MessageHandler.Register(new TitleCommand());
            MessageHandler.Register(new PCRRunCommand());

            MessageHandler.Register(new DDCommand());
            MessageHandler.Register(new CDCommand());
            MessageHandler.Register(new CCDCommand());
            MessageHandler.Register(new SLCommand());
            MessageHandler.Register(new SCCommand());
            MessageHandler.Register(new TBCommand());
            MessageHandler.Register(new RCCommand());
            MessageHandler.Register(new CPMCommand());

            CommandHelper.Register<AdditionalCommands.随机禁言>();
            CommandHelper.Register<AdditionalCommands.泰拉在线>();
            CommandHelper.Register<AdditionalCommands.泰拉资料>();
            CommandHelper.Register<AdditionalCommands.封>();
            CommandHelper.Register<AdditionalCommands.注册>();
            CommandHelper.Register<AdditionalCommands.在线排行>();
            CommandHelper.Register<AdditionalCommands.物品排行>();
            CommandHelper.Register<AdditionalCommands.财富排行>();
            CommandHelper.Register<AdditionalCommands.渔夫排行>();
            CommandHelper.Register<AdditionalCommands.死亡排行>();
            CommandHelper.Register<AdditionalCommands.用户>();
            CommandHelper.Register<AdditionalCommands.解>();
            CommandHelper.Register<AdditionalCommands.重置>();
            CommandHelper.Register<AdditionalCommands.切换>();
            CommandHelper.Register<AdditionalCommands.绑定>();
            CommandHelper.Register<AdditionalCommands.执行>();
            CommandHelper.Register<AdditionalCommands.解绑>();
            CommandHelper.Register<AdditionalCommands.开启前缀检测>();
            CommandHelper.Register<AdditionalCommands.关闭前缀检测>();
            CommandHelper.Register<AdditionalCommands.开启自动清人>();
            CommandHelper.Register<AdditionalCommands.关闭自动清人>();
            CommandHelper.Register<AdditionalCommands.加入黑名单>();
            CommandHelper.Register<AdditionalCommands.移除黑名单>();
            CommandHelper.Register<AdditionalCommands.黑名单列表>();
            CommandHelper.Register<AdditionalCommands.服务器列表>();
            CommandHelper.Register<AdditionalCommands.解ip>();
            CommandHelper.Register<AdditionalCommands.封ip>();
            CommandHelper.Register<AdditionalCommands.saveall>();

            MessageHandler.Register(new R18AllowedCommand());
            MessageHandler.Register(new NormalAllowedCommand());
            MessageHandler.Register(new SetuCommand());
            MessageHandler.Register(new ZMCCommand());
            MessageHandler.Register(new AntirevokeCommand());

            MessageHandler.Register(Configuration.GetConfig<ReplyHandler>());
            MessageHandler.Register(new RepeatHandler());

            foreach (var schedule in Configuration.GetConfig<TimeConfiguration>().t)
            {
                var s = schedule;
                ScheduleManager.QueueTimed(() =>
                {
                    session.SendGroupMessageAsync(s.group, Utils.GetMessageChain(s.message));
                }, s.delay);
            }

            GC.Collect();
            MessageHandler.booted = true;
        }

        public static async Task Main(string[] args)
        {
            
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            var options = new MiraiHttpSessionOptions("127.0.0.1", 8080, "1234567890");

            await using var session = new MiraiHttpSession();

            session.AddPlugin(new MessageHandler());

            await session.ConnectAsync(options, long.Parse(args[0]));

            PluginInitialize(session);

            Console.WriteLine("connected to server");

            Thread.Sleep(int.MaxValue);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e.ExceptionObject);
        }
    }
}
