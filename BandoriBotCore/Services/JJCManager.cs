using BandoriBot.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GoWasmWrapper;
using WebAssembly;

namespace BandoriBot.Services
{
    using JsObject = Dictionary<string, object>;
    public sealed class JJCManager : IDisposable
    {
        public static JJCManager Instance = new JJCManager(Path.Combine("jjc"));

#pragma warning disable CS0649
        class CommentModel
        {
            public string parent;
            public string id;
            public DateTime date;
            public string msg, nickname;
            public int avatar;

            public CommentModel(Result parent, Comment comment)
            {
                id = comment.id;
                date = comment.date;
                msg = comment.msg;
                nickname = comment.nickname;
                avatar = comment.avatar;
                this.parent = parent.id;
            }
        }

        class Character
        {
            public bool equip;
            public int id, star;

            public override string ToString()
            {
                return $"{id}-{star}-{equip}";
            }
        }

        class Comment
        {
            public string id;
            public DateTime date;
            public string msg, nickname;
            public int avatar;
        }

        class Result
        {
            public string id;
            public Character[] atk, def;
            public int up, down;
            public Comment[] comment;
            public DateTime updated;
        }
        class RespData
        {
            public Result[] result;
            public PageInfo page;
        }
        class PageInfo
        {
            public bool hasMore;
            public int page;
        }

        private readonly Dictionary<string, Image> textures;
        private readonly Dictionary<string, int> nicknames;
        private readonly Font font;

        private HttpClient client;

        private Trie<int> trie;

        private static string Normalize(string text)
        {
            return text.ToLower().Replace('（', '(').Replace('）', ')');
        }

        public JJCManager(string root)
        {
            try
            {
                textures = new Dictionary<string, Image>();
                nicknames = new Dictionary<string, int>();
                font = new Font(FontFamily.GenericMonospace, 15);

                foreach (var file in Directory.GetFiles(root))
                    if (file.EndsWith(".png"))
                        textures.Add(Path.GetFileNameWithoutExtension(file), Image.FromFile(file));

                trie = new Trie<int>();

                foreach (Match match in Regex.Matches(File.ReadAllText(Path.Combine(root, "chara_names.py")),
                             @"(\d\d\d\d): \[(.*?)\],"))
                {
                    var id = 100 * int.Parse(match.Groups[1].Value) + 1;// characters.SingleOrDefault(c => c.name.EndsWith(splits[2]));
                    foreach (var text in match.Groups[2].Value.Split(','))
                    {
                        var nickname = text.Trim(' ').Trim('"');
                        if (!nicknames.ContainsKey(nickname))
                        {
                            trie.AddWord(Normalize(nickname), id);
                            nicknames.Add(nickname, id);
                        }
                    }
                }
            }
            catch
            {

            }

            client = new HttpClient();

            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.88 Safari/537.36 Edg/87.0.664.66");
            client.DefaultRequestHeaders.Add("Referer", "https://pcrdfans.com/");
            client.DefaultRequestHeaders.Add("Origin", "https://pcrdfans.com");
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "empty");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "cors");
            client.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-site");
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            //client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("Accept-Language", "zh-CN,zh;q=0.9,en;q=0.8,en-GB;q=0.7,en-US;q=0.6");
            client.DefaultRequestHeaders.Remove("Expect");
            client.Timeout = new TimeSpan(0, 0, 10);
            /*
            pool.GetProxysFromAPIs();
            */
        }
        
        private Image GetTexture(Character c)
        {
            Image img;
            if (c.star == 6 && textures.TryGetValue((c.id + 60).ToString(), out img))
                return img;
            else if (c.star >= 3 && textures.TryGetValue((c.id + 30).ToString(), out img))
                return img;
            else
                return textures[(c.id + 10).ToString()];
        }

        private void DrawToGraphics(Graphics canvas, Character c, int offx, int offy)
        {
            canvas.DrawImage(GetTexture(c), new Rectangle(offx, offy, 100, 100));
            canvas.DrawImage(textures["search-li"], new Rectangle(offx, offy, 100, 100));
            for (int i = 0; i < c.star; ++i)
                canvas.DrawImage(textures["star"], new Rectangle(5 + 12 * i + offx, 80 + offy, 15, 15));
            if (c.equip) canvas.DrawImage(textures["arms"], new Rectangle(15 + offx, 15 + offy, 15, 15));
        }

        private void DrawToGraphics(Graphics canvas, Result t, int offx, int offy)
        {
            for (int i = 0; i < 5; ++i)
            {
                DrawToGraphics(canvas, t.atk[i], 110 * i + offx, offy);
                DrawToGraphics(canvas, t.def[i], 110 * i + 590 + offx, offy);
            }

            int y = 0;
            foreach (var cmt in t.comment.Take(5))
                canvas.DrawString($"[{cmt.nickname}]{cmt.msg}", font, Brushes.Black, 590 + 590 + offx, offy + 20 * (y++));

            canvas.DrawString($"顶：{t.up} 踩：{t.down} ", font, Brushes.Black, offx, offy + 100);
        }

        private Image GetImage(Result[] teams)
        {
            var n = teams.Length;
            var result = new Bitmap(1130 + 590, 120 * n + 20);
            var canvas = Graphics.FromImage(result);
            canvas.Clear(Color.White);
            for (int i = 0; i < n; ++i) DrawToGraphics(canvas, teams[i], 0, i * 120);
            canvas.DrawString($"powered by www.pcrdfans.com", font, Brushes.Black, 0, 120 * n);
            canvas.Dispose();
            return result;
        }

        private static string GenNonce()
        {
            const string chars = "0123456789abcdefghijklmnopqrstuvwxyz";
            var rand = new Random();

            return new string(Enumerable.Range(0, 16).Select(_ => chars[rand.Next(36)]).ToArray());
        }
        private static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (long)ts.TotalSeconds;
        }

        internal GoWrapper wrapper;

        private static double calcHash(string str)
        {
            var text = Encoding.ASCII.GetBytes(str);
            uint _0x473e93, _0x5d587e;
            for (_0x473e93 = 0x1bf52, _0x5d587e = (uint)text.Length; _0x5d587e != 0;)
                _0x473e93 = 0x309 * _0x473e93 ^ text[--_0x5d587e];
            return _0x473e93 >> 0x3;
        }

        private static double myHash(string str)
        {
            var text = Encoding.ASCII.GetBytes(str);
            uint _0x473e93, _0x5d587e;
            for (_0x473e93 = 0x202, _0x5d587e = (uint)text.Length; _0x5d587e != 0;)
                _0x473e93 = 0x72 * _0x473e93 ^ text[--_0x5d587e];
            return _0x473e93 >> 0x3;
        }

        private string version;

        private async Task UpdateVersion()
        {
            var cur = JObject.Parse(await client.GetStringAsync("https://api.pcrdfans.com/x/v1/search")).Value<string>("version");
            if (cur != version)
            {
                version = cur;

                await File.WriteAllBytesAsync("pcrd.wasm", await client.GetByteArrayAsync("https://pcrdfans.com/pcrd.wasm"));

                wrapper = new GoWrapper(Module.ReadFromBinary("pcrd.wasm"))
                {
                    Global =
                    {
                        ["myhash"] = new Func<string, double>(myHash),
                        ["location"] = new JsObject
                        {
                            ["host"] = "pcrdfans.com",
                            ["hostname"] = "pcrdfans.com",

                        }
                    }
                };
            }

        }

        public async Task<string> Callapi(string text)
        {
            string prefix = "";
            var indexes = trie.WordSplit(Normalize(text));
            if (indexes.Item1.Length == 0) return string.Empty;
            if (indexes.Item1.Length > 5)
                throw new ApiException("角色数超过了五个！");
            else if (indexes.Item1.Length < 5)
                prefix = "**角色数少于五个**\n" +
                    (indexes.Item2.Length > 0 ? $"未能识别的名字：{string.Join(',', indexes.Item2.Where(s => !string.IsNullOrWhiteSpace(s)))}\n" : "");

            await UpdateVersion();
            this.Log(Models.LoggerLevel.Debug, $"chara id = {string.Join(",", indexes.Item1)}");

            var nonce = GenNonce();
            var json = new JObject
            {
                ["def"] = new JArray(indexes.Item1),
                ["language"] = 0,
                ["nonce"] = nonce,
                ["page"] = 1,
                ["region"] = 1,
                ["sort"] = 1,
                ["ts"] = GetTimeStamp()
            };

            var sign = wrapper.RunEvent(1, new object[]
            {
                json.ToString(Formatting.None),
                nonce,
                calcHash(nonce)
            }) as string;

            json = new JObject
            {
                ["_sign"] = sign,
                ["def"] = json["def"],
                ["language"] = json["language"],
                ["nonce"] = json["nonce"],
                ["page"] = json["page"],
                ["region"] = json["region"],
                ["sort"] = json["sort"],
                ["ts"] = json["ts"]
            };


            RespData result;
            JObject raw = null;

            try
            {
                try
                {
                    raw = JObject.Parse(client.PostAsync($"https://api.pcrdfans.com/x/v1/search",
                        new StringContent(json.ToString(Formatting.None)
                    , Encoding.UTF8, "application/json")).Result.Content.ReadAsStringAsync().Result);
                    result = raw["data"].ToObject<RespData>();

                }
                catch
                {
                    throw;
                    //raw = ProxyPost(json);
                    //result = raw["data"].ToObject<RespData>();
                }
            }
            catch
            {
                return $"pcrd err:{raw}";
            }

            return prefix + Utils.GetImageCode(GetImage(result.result.Take(10).ToArray()).Resize(0.5f));
        }

        public void Dispose()
        {
            foreach (var pair in textures) pair.Value.Dispose();
        }
    }
}
