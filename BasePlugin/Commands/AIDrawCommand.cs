using System;
using System.Collections.Generic;
using System.Drawing;
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
using BandoriBot.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace BasePlugin.Commands
{
    public class TagRecordContext : DbContext
    {
        public static TagRecordContext Instance => _instance ??= new TagRecordContext();
        
        private static TagRecordContext _instance;

        public DbSet<TagRecord> TagRecords { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=tagrecords.db");
        }
    }
    public class TagRecord
    {
        public long id { get; set; }
        public long count { get; set; }
        public string tag { get; set; }
    }
    
    internal class AIDrawCommand : ICommand
    {
        public List<string> Alias => new() { "ai画图", "生成涩图", "生成色图" };
        [ThreadStatic]
        private static HttpClient client;

        public AIDrawCommand()
        {
            if (File.Exists("aidraw")) return;
            File.WriteAllBytes("aidraw", Array.Empty<byte>());
            TagRecordContext.Instance.Database.EnsureCreated();
            var files = Directory.EnumerateFiles("imagecache").ToArray();
            for (int i = 0; i < files.Length; ++i)
            {
                using var img = Image.FromFile(files[i]);
                var value = img.PropertyItems[1].Value;
                if (value == null) continue;
                var tags = Encoding.UTF8.GetString(value.Where(b => b != 0).ToArray()).Split(',');
                foreach (var tag in tags)
                {
                    AddTag(tag);
                }
            }
        }

        private void AddTag(string tag)
        {
            tag = tag.ToLower().Trim();
            while (tag.StartsWith('{') && tag.EndsWith('}') ||
                   tag.StartsWith('(') && tag.EndsWith(')'))
                tag = tag.Substring(1, tag.Length - 2);
            if (tag == "novelai") return;
            lock (this)
            {
                var tag0 = TagRecordContext.Instance.TagRecords.FirstOrDefault(t => t.tag == tag);
                if (tag0 == null)
                {
                    TagRecordContext.Instance.TagRecords.Add(new TagRecord()
                    {
                        tag = tag,
                        count = 1
                    });
                }
                else
                {
                    this.Log(LoggerLevel.Info, $"tag added: {tag}");
                    tag0.count++;
                }
                TagRecordContext.Instance.SaveChanges();
            }
        }
        public static HttpClient Client => client ??= new HttpClient();
        public async Task Run(CommandArgs args)
        {
            return;
            if (Configuration.GetConfig<Blacklist2>().t.Contains(args.Source.FromQQ.ToString()))
            {
                await args.Callback($"[mirai:at={args.Source.FromQQ}]你的账号已被封禁");
                return;
            }
            var arg = args.Arg.Trim().Decode();
            var text = arg.ToLower();
            var array = new string[4] { "nsfw", "r18", "nude", "nake" };
            if (string.IsNullOrEmpty(arg))
            {
                var rankboard = string.Join("\n", TagRecordContext.Instance.TagRecords.OrderByDescending(t => t.count).Take(10)
                    .AsEnumerable()
                    .Select((t, i) => $"{i + 1}.{t.tag}({t.count} times)"));
                await args.Callback($"[mirai:at={args.Source.FromQQ}]你没有加参数哦，目前tag排行榜：\n{rankboard}".ToImageText());
            }
            foreach (var t in array)
            {
                if (text.Contains(t))
                {
                    Configuration.GetConfig<Blacklist2>().t.Add(args.Source.FromQQ.ToString());
                    await args.Callback($"[mirai:at={args.Source.FromQQ}]你的账号由于使用违禁词作图已被封禁");
                    return;
                }
            }
            new Thread(()=>
            {
                try
                {
                    var dictionary =
                        (arg.Split('&').Select(p => (p, p.IndexOf("=", StringComparison.Ordinal))))
                        .ToDictionary<(string, int), string, string>(
                            ((string p, int) p) => (p.Item2 != -1) ? p.p.Substring(0, p.Item2).ToLower() : "tags",
                            delegate((string p, int) p)
                            {
                                string text3;
                                if (p.Item2 == -1)
                                {
                                    (text3, _) = p;
                                }
                                else
                                {
                                    text3 = p.p.Substring(p.Item2 + 1);
                                }

                                return text3.Replace("，", ",");
                            });
                    if (dictionary.TryGetValue("tags", out var tags))
                    {
                        foreach (var tag in tags.Split(",")) AddTag(tag);
                    }
                    dictionary.Remove("r18");
                    var text2 = "http://91.217.139.190:5010/got_image?" +
                                string.Join("&",
                                    dictionary.Select(p =>
                                        HttpUtility.UrlEncode(p.Key) + "=" + HttpUtility.UrlEncode(p.Value))) +
                                "&token=bEWDO2hjfIoJXeRMKqpQB1yzu86CxAtw";
                    Utils.Log((object)this, (LoggerLevel)10, (object)text2);
                    var path = Path.Combine("imagecache", $"cache{new Random().Next()}.jpg");
                    var result = Client.GetByteArrayAsync(text2).Result;
                    var arg2 = string.Empty;
                    try
                    {
                        Image image;
                        using (var stream = new MemoryStream(result))
                        {
                            image = Image.FromStream(stream);
                        }
                        arg2 = $"seed:{JObject.Parse(Encoding.UTF8.GetString(image.PropertyItems[4].Value))["seed"]}";
                    }
                    catch
                    {
                    }
                    File.WriteAllBytes(path, result);
                    args.Callback($"[mirai:at={args.Source.FromQQ}][mirai:imagepath={Path.GetFullPath(path)}]{arg2}").Wait();
                }
                catch (Exception ex)
                {
                    try
                    {
                        while (ex.InnerException != null) ex = ex.InnerException;
                        args.Callback("生成出错了：" + ex.Message + "，绝对是路路的服务器不好！");
                    }
                    catch
                    {
                    }
                }
            }).Start();
        }
    }
}
