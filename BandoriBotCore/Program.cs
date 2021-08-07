using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Models;
using BandoriBot.Services;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using SekaiClient.Datas;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace BandoriBot
{
    class Program
    {
        public static object SekaiFile = new object();

        private static void PluginInitialize(MiraiHttpSession session)
        {
            MessageHandler.session = session;

            Configuration.Register<AntirevokePlus>();
            Configuration.Register<Activation>();
           // Configuration.Register<MainServerConfig>();
            Configuration.Register<Delay>();
            Configuration.Register<MessageStatistic>();
            Configuration.Register<ReplyHandler>();
            Configuration.Register<Whitelist>();
            Configuration.Register<Blacklist>();
            Configuration.Register<BlacklistF>();
            Configuration.Register<TitleCooldown>();
            //Configuration.Register<PCRConfig>();
            Configuration.Register<R18Allowed>();
            Configuration.Register<NormalAllowed>();
            Configuration.Register<AccountBinding>();
            //Configuration.Register<ServerManager>();
            Configuration.Register<TimeConfiguration>();
            Configuration.Register<GlobalConfiguration>();
            Configuration.Register<Antirevoke>();
            Configuration.Register<SetuConfig>();
            Configuration.Register<Save>();
            Configuration.Register<CarTypeConfig>();
            Configuration.Register<SubscribeConfig>();
            Configuration.Register<PermissionConfig>();
            Configuration.Register<Pipe>();
            Configuration.Register<TokenConfig>();
            Configuration.Register<SekaiCache>();
            //Configuration.Register<PeriodRank>();

            MessageHandler.Register<SetTokenCommand>();
            MessageHandler.Register<CarHandler>();
            MessageHandler.Register(Configuration.GetConfig<ReplyHandler>());
            MessageHandler.Register<WhitelistHandler>();
            MessageHandler.Register<RepeatHandler>();
            MessageHandler.Register(Configuration.GetConfig<MessageStatistic>());
            //MessageHandler.Register(Configuration.GetConfig<MainServerConfig>());

            MessageHandler.Register<YCM>();
            MessageHandler.Register<QueryCommand>();
            MessageHandler.Register<ReplyCommand>();
            MessageHandler.Register<FindCommand>();
            MessageHandler.Register<DelayCommand>();
            MessageHandler.Register<AntirevokePlusCommand>();
            MessageHandler.Register<SekaiCommand>();
            MessageHandler.Register<SekaiPCommand>();
            MessageHandler.Register<WhitelistCommand>();
            MessageHandler.Register<GachaCommand>();
            MessageHandler.Register<GachaListCommand>();
            MessageHandler.Register<Activate>();
            MessageHandler.Register<Deactivate>();
            MessageHandler.Register<BlacklistCommand>();
            MessageHandler.Register<TitleCommand>();
            MessageHandler.Register<CarTypeCommand>();
            MessageHandler.Register<SekaiLineCommand>();
            MessageHandler.Register<SekaiGachaCommand>();
            MessageHandler.Register<PermCommand>();
            MessageHandler.Register<SendCommand>();
            MessageHandler.Register<RecordCommand>();
            /*
            MessageHandler.Register<RCCommand>();
            MessageHandler.Register<CPMCommand>();

            CommandHelper.Register<AdditionalCommands.随机禁言>();
            CommandHelper.Register<AdditionalCommands.泰拉在线>();
            CommandHelper.Register<AdditionalCommands.泰拉资料>();
            CommandHelper.Register<AdditionalCommands.封>();
            CommandHelper.Register<AdditionalCommands.泰拉注册>();
            CommandHelper.Register<AdditionalCommands.泰拉在线排行>();
            CommandHelper.Register<AdditionalCommands.泰拉物品排行>();
            CommandHelper.Register<AdditionalCommands.泰拉财富排行>();
            CommandHelper.Register<AdditionalCommands.泰拉渔夫排行>();
            CommandHelper.Register<AdditionalCommands.泰拉重生排行>();
            CommandHelper.Register<AdditionalCommands.泰拉玩家>();
            CommandHelper.Register<AdditionalCommands.泰拉背包>();
            CommandHelper.Register<AdditionalCommands.解>();
            CommandHelper.Register<AdditionalCommands.重置>();
            CommandHelper.Register<AdditionalCommands.泰拉切换>();
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
            */
            MessageHandler.Register<R18AllowedCommand>();
            MessageHandler.Register<NormalAllowedCommand>();
            MessageHandler.Register<SetuCommand>();
            MessageHandler.Register<ZMCCommand>();
            MessageHandler.Register<AntirevokeCommand>();
            MessageHandler.Register<SubscribeCommand>();
            RecordDatabaseManager.InitDatabase();

            if (File.Exists("sekai"))
            {
            }

            Configuration.LoadAll();

            foreach (var schedule in Configuration.GetConfig<TimeConfiguration>().t)
            {
                var s = schedule;
                ScheduleManager.QueueTimed(async () =>
                {
                    await session.SendGroupMessageAsync(s.group, await Utils.GetMessageChain(s.message, async p => await session.UploadPictureAsync(UploadTarget.Group, p)));
                }, s.delay);
            }

            GC.Collect();
            MessageHandler.booted = true;

        }

        public static async Task Main(string[] args)
        {
            string authkey = File.ReadAllText("authkey.txt");

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            
            await using var session = new MiraiHttpSession();

            session.AddPlugin(new MessageHandler());
            
            PluginInitialize(session);

            var options = new MiraiHttpSessionOptions("localhost", int.Parse(args[1]), authkey);

            await session.ConnectAsync(options, long.Parse(args[0]));

            Console.WriteLine("connected to server");

            new Thread(() => Apis.Program.Main2(args)).Start();

            while (true)
            {
                var r = Console.ReadLine();
                if (r == "exit")
                {
                    RecordDatabaseManager.Close();
                    Environment.Exit(0);
                }
            }
        }

        private static void CurrentDomain_FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
        {
            var ex = e.Exception;
            while (ex is AggregateException e2) ex = e2.InnerException;
            if (ex is ApiException) return;
            if (ex is IOException) return;

            Console.WriteLine(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            RecordDatabaseManager.Close();
            Console.WriteLine(e.ExceptionObject);
        }

        private static async Task Testing()
        {
            Console.WriteLine(await JJCManager.Instance.Callapi("环奈水黑布丁空花望"));
            Environment.Exit(0);
        }
    }
}
