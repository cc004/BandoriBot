using BandoriBot.Config;
using BandoriBot.DataStructures;
using BandoriBot.Handler;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class SubscribeCommand : ICommand, ISession
    {
        private readonly SubscribeConfig config;
        public MiraiHttpSession Session { get; set; }

        public SubscribeCommand()
        {
            config = Configuration.GetConfig<SubscribeConfig>();
            YCM.OnNewCar += car => OnNewCar(CarType.Bandori, car);
            CarHandler.OnNewCar += car => OnNewCar(CarType.Sekai, car);
        }

        private async void OnNewCar(CarType type, Car obj)
        {
            try
            {
                Dictionary<CarType, string[]> targets;

                lock (config)
                    targets = config.t.GroupBy(pair => pair.Value).ToDictionary(group => group.Key, group => group.Select(pair => pair.Key).ToArray());

                var msgchain = new IMessageBase[]
                {
                    new PlainMessage(obj.ToString())
                };

                foreach (var target in targets[type])
                {
                    try
                    {
                        var num = long.Parse(target.Substring(1));

                        switch (target[0])
                        {
                            case 'g':
                                await Session.SendGroupMessageAsync(num, msgchain);
                                break;
                            case 'q':
                                await Session.SendFriendMessageAsync(num, msgchain);
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        this.Log(Models.LoggerLevel.Error, e);
                    }
                }
            }
            catch (Exception e)
            {
                this.Log(Models.LoggerLevel.Error, e);
            }
        }

        public List<string> Alias => new List<string> { "/subscribe" };

        public async Task Run(CommandArgs args)
        {
            var arg = args.Arg.Trim();

            if (args.Source.FromGroup == 0)
            {
                var type = Enum.Parse<CarType>(arg);

                config[$"q{args.Source.FromQQ}"] = type;

                await args.Callback($"subscribe type of account {args.Source.FromQQ} set to {type}");
            }
            else
            {
                if (!await args.Source.CheckPermission(args.Source.FromGroup))
                {
                    await args.Callback("access denied.");
                    return;
                }

                var type = Enum.Parse<CarType>(arg);

                config[$"g{args.Source.FromGroup}"] = type;

                await args.Callback($"subscribe type of group {args.Source.FromGroup} set to {type}");
            }
        }
    }

}
