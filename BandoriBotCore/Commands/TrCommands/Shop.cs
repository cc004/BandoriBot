using System.Linq;
using BandoriBot.Config;
using Newtonsoft.Json.Linq;
using BandoriBot.Terraria;

#pragma warning disable CS0028

namespace BandoriBot.Commands.Terraria
{
    public static partial class TerrariaCommands
    {
        public class 泰拉商店
        {
            public static void Main(CommandArgs args,string cmd,int num)
            {
                var server = Configuration.GetConfig<ServerManager>().GetServer(args);
                string res = "还没写，爬！";
                switch (cmd)
                {
                    case "下架":
                        {

                        }
                        break;
                    case "交易列表":
                        {
                            var arr = (server.RunRest("/economy/getshopitems") as JObject)["market"];
                            var list = arr.ToArray();
                            int totalpage = list.Length / 10 + (list.Length % 10 > 0 ? 1 : 0);
                            res = "交易列表：\r";
                            num--;
                            if(num < 0)
                            {
                                num = 0;
                            }
                            for (int i = num * 10; i < ((num + 1) * 10 + 1 < list.Length ? (num + 1) * 10 + 1 : list.Length); i++)
                            {
                                res += $"{i}.物品:{list[i]["ItemID"]}x{list[i]["ItemStack"]} " +
                                    (int.Parse(list[i]["ItemPrefix"].ToString()) == 0 ? "" : $"前缀:{list[i]["ItemPrefix"]}") +
                                    $"价格:{list[i]["Price"]} 出售者:{list[i]["Owner"]}\r";
                            }
                            res += $"==页码[{num + 1}/{totalpage}]";
                        }
                        break;
                    case "商店列表":
                        {
                            var arr = (server.RunRest("/economy/getshopitems") as JObject)["shop"];
                            var list = arr.ToArray();
                            int totalpage = list.Length / 10 + (list.Length % 10 > 0 ? 1 : 0);
                            res = "商店列表：\r";
                            num--;
                            if (num < 0)
                            {
                                num = 0;
                            }
                            for (int i = num * 10; i < ((num + 1) * 10 + 1 < list.Length ? (num + 1) * 10 + 1 : list.Length); i++)
                            {
                                res += $"{i}.物品:{list[i]["ItemID"]}x{list[i]["ItemStack"]} " +
                                    (int.Parse(list[i]["ItemPrefix"].ToString()) == 0 ? "" : $"前缀:{list[i]["ItemPrefix"]}") +
                                    $"价格:{list[i]["Price"]}\r";
                            }
                            res += $"==页码[{num + 1}/{totalpage}]";
                        }
                        break;
                    case "购买":
                        {

                        }
                        break;
                    default:
                        {
                            res = "没这功能，爬！";
                        }
                        break;
                }
                args.Callback(res);
            }
            public static void 上架(CommandArgs args,int id,int stack,int price)
            {
                string res = "还没写，爬！";
                args.Callback(res);
            }
            public static void Main(CommandArgs args)
            {
                args.Callback("指令格式：\r" +
                    "泰拉商店 商店列表 页码\r" +
                    "泰拉商店 交易列表 页码\r" +
                    "泰拉商店 下架 编号\r" +
                    "泰拉商店 上架 物品ID 数量 价格\r" +
                    "泰拉商店 购买 序号");
            }
        }
    }
}
