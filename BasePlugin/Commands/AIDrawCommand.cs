using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BandoriBot;
using BandoriBot.Commands;
using BandoriBot.Config;

namespace BasePlugin.Commands
{
    internal class AIDrawCommand : ICommand
    {
        public List<string> Alias => new() { "ai画图", "生成涩图", "生成色图" };
        [ThreadStatic]
        private static HttpClient client;

        public static HttpClient Client => client ??= new HttpClient();
        public async Task Run(CommandArgs args)
        {
            new Thread(() =>
            {
                var tag = HttpUtility.UrlEncode(args.Arg);
                var url =
                    $"http://91.217.139.190:5010/got_image?tags={tag}&token=bEWDO2hjfIoJXeRMKqpQB1yzu86CxAtw";
                var path = Path.Combine("imagecache", $"cache{new Random().Next()}.jpg");
                File.WriteAllBytes(path, Client.GetByteArrayAsync(url).Result);
                args.Callback($"[mirai:imagepath={Path.GetFullPath(path)}]").Wait();
            }).Start();
        }
    }
}
