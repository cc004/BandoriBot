using BandoriBot.Config;
using BandoriBot.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BandoriBot.Commands
{
    public class R18AllowedCommand : HashCommand<R18Allowed, long>
    {
        public override List<string> Alias => new List<string> { "/r18" };
        public override async Task Run(CommandArgs args)
        {
            if (args.Source.FromQQ != 1176321897L) return;
            await base.Run(args);
        }
    }

    public class NormalAllowedCommand : HashCommand<NormalAllowed, long>
    {
        public override List<string> Alias => new List<string> { "/normal" };
        protected override long GetTarget(long value) => value;

        public override async Task Run(CommandArgs args)
        {
            await base.Run(args);
        }
    }

    public class SetuCommand : ICommand
    {
        private readonly Random rand = new Random();
        
        private static readonly HttpClient client = new HttpClient();

        static SetuCommand()
        {
            string apikey;
            try
            {
                apikey = File.ReadAllText("acgmx_apikey.txt");
            }
            catch
            {
                apikey = null;
            }

            client.DefaultRequestHeaders.Add("token", apikey);
        }
        private class SearchResult
        {
            public int pid, uid;
            public string uri, origin;
            public int sanity;
            public int bookmark;
        }

        public List<string> Alias => new List<string> { "来点颜色" };

        private static async Task<JArray> CallApi(Dictionary<string, string> param)
        {
            var sb = new StringBuilder();
            sb.Append("https://api.acgmx.com/public/search?");
            foreach (var pair in param)
                sb.Append($"{pair.Key}={HttpUtility.UrlEncode(pair.Value)}&");
            sb.Remove(sb.Length - 1, 1);


            var resp = await client.GetAsync(sb.ToString());

            JObject result;

            result = JObject.Parse(await resp.Content.ReadAsStringAsync());

            return result["illusts"] as JArray;
        }

        public static async Task<byte[]> GetImage(string uri)
        {
            uri = uri.Replace("i.pximg.net", "i.pixiv.cat");

            using (var client = new HttpClient())
                return await client.GetByteArrayAsync(uri);
        }

        private static async Task<IEnumerable<SearchResult>> SearchOnePage(string keyword, int offset)
        {
            try
            {
                return (await CallApi(new Dictionary<string, string>
                {
                    ["q"] = keyword,
                    ["offset"] = offset.ToString()
                })).Select(token => new SearchResult
                {
                    bookmark = token.Value<int>("total_bookmarks"),
                    sanity = token.Value<int>("sanity_level"),
                    uri = token["image_urls"].Value<string>("medium"),
                    pid = token.Value<int>("id"),
                    uid = token["user"].Value<int>("id"),
                    origin = token["image_urls"].Value<string>("large")
                });
            }
            catch
            {
                return new List<SearchResult>();
            }
        }

        private static Dictionary<string, List<SearchResult>> cache = new Dictionary<string, List<SearchResult>>();
        private static Dictionary<string, Tuple<IEnumerable<SearchResult>, int>> processing = new Dictionary<string, Tuple<IEnumerable<SearchResult>, int>>();

        private static async Task<List<SearchResult>> SearchAll(string keyword)
        {
            List<SearchResult> result = null;
            const int page_limit = 250;
            const int page_offset = 50;
            const int bookmark_filter = 50;

            while (true)
            {
                if (processing.TryGetValue(keyword, out var v))
                {
                    int i = v.Item2 + page_offset;
                    var lst = v.Item1;
                    if (i < page_limit)
                    {
                        var t = (await SearchOnePage(keyword, i)).ToList();
                        if (t.Count == 0) i = page_limit;
                        else lst = lst.Concat(t);
                    }

                    result = lst.Where(res => res.bookmark > bookmark_filter).ToList();

                    if (i == page_limit)
                    {
                        cache[keyword] = lst.ToList();
                        processing.Remove(keyword);
                    }
                    else
                        processing[keyword] = new Tuple<IEnumerable<SearchResult>, int>(lst, i);

                    if (result.Count > 0) return result;
                }
                else if (cache.TryGetValue(keyword, out var v2))
                    return v2.Where(res => res.bookmark > 50).ToList();
                else
                    processing.Add(keyword, new Tuple<IEnumerable<SearchResult>, int>(new List<SearchResult>(), 0));
            }
        }

        public async Task Run(CommandArgs args)
        {
            var flag = Configuration.GetConfig<NormalAllowed>().hash.Contains(args.Source.FromGroup);
            var flag2 = Configuration.GetConfig<R18Allowed>().hash.Contains(args.Source.FromGroup);

            if (string.IsNullOrWhiteSpace(args.Arg))
            {
                var pics = Configuration.GetConfig<SetuConfig>().t.Where(pic => pic.r18 ? flag2 : flag).ToArray();
                if (pics.Length == 0)
                {
                    await args.Callback("图片库为空或者你所在的群没有权限！");
                    return;
                }
                var pic = pics[rand.Next(pics.Length)];
                var client = new HttpClient();
                var imgres = Image.FromStream(client.GetStreamAsync(pic.url
                    .Replace("img-original", "c/540x540_70/img-master")
                    .Split("_p0.").First() + "_p0_master1200.jpg").Result);

                await args.Callback($"作品id: {pic.pid}\n" +
                    $"画师id: {pic.uid}\n" +
                    $"神秘链接: {pic.url}\n" +
                    Utils.GetImageCode(imgres));
                return;
            }

            var result = await SearchAll(args.Arg.Trim());

            result = result.Where(t => t.sanity == 2 && flag || t.sanity != 2 && flag2).ToList();

            if (result.Count == 0)
            {
                await args.Callback($"找不到\"{args.Arg.Trim()}\"的图片!");
                return;
            }

            var piece = result[new Random().Next(result.Count)];
            var img = GetImage(piece.uri).Result;
            await args.Callback($"{args.Arg.Trim()}:\n" +
                    $"作品id: {piece.pid}\n" +
                    $"画师id: {piece.uid}\n" +
                    $"神秘链接: {piece.origin}\n" +
                    Utils.GetImageCode(img));
        }
    }
}
