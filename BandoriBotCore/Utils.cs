using BandoriBot.Handler;
using BandoriBot.Models;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mirai_CSharp;
using Mirai_CSharp.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Mirai_CSharp.Models.PokeMessage;

namespace BandoriBot
{
    public static class Utils
    {

        public static string FindAtMe(string origin, out bool isat, long qq)
        {
            var at = $"[mirai:at={qq}]";
            isat = origin.Contains(at);
            return origin.Replace(at, "");
        }

        public static string TryGetValueStart<T>(IEnumerable<T> dict, Func<T, string> conv, string start, out T value)
        {
            var matches = new List<Tuple<string, T>>();
            foreach (var pair in dict)
            {
                var key = conv(pair);
                if (key.StartsWith(start))
                {
                    if (key == start)
                    {
                        value = pair;
                        return null;
                    }
                    matches.Add(new Tuple<string, T>(key, pair));
                }
            }

            value = default;

            if (matches.Count == 0)
            {
                return $"No matches found for `{start}`";
            }

            if (matches.Count > 2)
            {
                return $"Multiple matches found : \n{string.Concat(matches.Select((pair) => pair.Item1 + "\n"))}";
            }

            value = matches[0].Item2;
            return null;
        }

        private static Regex codeReg = new Regex(@"^(.*?)\[(.*?)=(.*?)\](.*)$", RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.Compiled);
        
        public static async Task<IMessageBase[]> GetMessageChain(string msg, Func<string, Task<ImageMessage>> picUploader)
        {
            Match match;
            List<IMessageBase> result = new List<IMessageBase>();

            while ((match = codeReg.Match(msg)).Success)
            {
                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                    result.Add(new PlainMessage(match.Groups[1].Value.Decode()));
                var val = match.Groups[3].Value;
                switch (match.Groups[2].Value)
                {
                    case "mirai:at": result.Add(new AtMessage(long.Parse(val))); break;
                    case "mirai:imageid": result.Add(new ImageMessage(val.Decode(), "", "")); break;
                    case "mirai:imageurl": result.Add(new ImageMessage("", val.Decode(), "")); break;
                    case "mirai:imagepath": result.Add(await picUploader(Path.GetFullPath(val.Decode()))); break;
                    case "mirai:atall": result.Add(new AtAllMessage());break;
                    case "mirai:json": result.Add(new JsonMessage(val.Decode())); break;
                    case "mirai:xml": result.Add(new XmlMessage(val.Decode())); break;
                    case "mirai:poke": result.Add(new PokeMessage(Enum.Parse<PokeType>(val))); break;
                    case "mirai:face": result.Add(new FaceMessage(int.Parse(val), "")); break;
                    case "CQ:at,qq": result.Add(new AtMessage(long.Parse(val))); break;
                    case "CQ:face,id": result.Add(new FaceMessage(int.Parse(val), "")); break;
                }
                msg = match.Groups[4].Value;
            }

            if (!string.IsNullOrEmpty(msg)) result.Add(new PlainMessage(msg.Decode()));

            return result.ToArray();
        }

        /*
public static string FixImage(string origin)
{
   return new Regex(@"\[CQ:image,file=(.*?)\]")
       .Replace(origin, (match) =>
       {
           try
           {
               var path = Path.Combine("", @$"..\..\image\{match.Groups[1].Value}.cqimg");
               var img = IniObject.Load(path);
               return $"<{img["image"]["url"]}>";
           }
           catch
           {
               return "<图片信息获取失败>";
           }
       });
}
*/
        public static string FixRegex(string origin)
        {
            return origin.Replace("[", @"\[").Replace("]", @"\]").Replace("&#91;", "[").Replace("&#93;", "]");
        }

        public static Bitmap LoadImage(this string path)
        {
            return Image.FromFile(path) as Bitmap;
        }

        public static async Task<string> GetName(this MiraiHttpSession session, long group, long qq)
        {
            try
            {
                return (await session.GetGroupMemberInfoAsync(qq, group)).Name;
            }
            catch (Exception e)
            {
                Log(LoggerLevel.Error, e.ToString());
                return qq.ToString();
            }
        }

        public static async Task<string> GetName(this Source source)
            => await source.Session.GetName(source.FromGroup, source.FromQQ);

        internal static string GetCQMessage(IEnumerable<IMessageBase> chain)
        {
            return string.Concat(chain.Select(msg => GetCQMessage(msg)));
        }

        public static string Encode(this string str)
        {
            return str.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;");
        }

        public static string Decode(this string str)
        {
            return str.Replace("&#91;", "[").Replace("&#93;", "]").Replace("&amp;", "&");
        }

        private static string GetCQMessage(IMessageBase msg)
        {
            switch (msg)
            {
                case FaceMessage face:
                    return $"[mirai:face={face.Id}]";
                case PlainMessage plain:
                    return plain.Message.Encode();
                case JsonMessage json:
                    return $"[mirai:json={json.Json.Encode()}]";
                case XmlMessage xml:
                    return $"[mirai:xml={xml.Xml.Encode()}";
                case AtMessage at:
                    return $"[mirai:at={at.Target}]";
                case ImageMessage img:
                    return img.ImageId != null ? $"[mirai:imageid={img.ImageId}]" : $"[mirai:imageurl={img.Url.Encode()}]";
                case AtAllMessage atall:
                    return $"[mirai:atall=]";
                case PokeMessage poke:
                    return $"[mirai:poke={poke.Name}]";
                case QuoteMessage quote:
                    return "";
                case SourceMessage _:
                    return "";
                default:
                    return msg.ToString().Encode();
            }
        }

        public static string GetImageCode(byte[] img)
        {
            var path = Path.Combine("imagecache", $"cache{new Random().Next()}.jpg");
            File.WriteAllBytes(path, img);
            return $"[mirai:imagepath={path}]";
        }

        public static Image Resize(this Image img, float scale)
        {
            var result = new Bitmap(img, new Size((int)(img.Width * scale), (int)(img.Height * scale)));
            img.Dispose();
            return result;
        }

        public static string GetImageCode(Image img)
        {
            var path = Path.Combine("imagecache", $"cache{new Random().Next()}.jpg");
            img.Save(path);
            return $"[mirai:imagepath={path}]";
        }

        public static void Log(this object o, LoggerLevel level, object s)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = level switch
                {
                    LoggerLevel.Debug => ConsoleColor.White,
                    LoggerLevel.Info => ConsoleColor.Green,
                    LoggerLevel.Warn => ConsoleColor.Yellow,
                    LoggerLevel.Error => ConsoleColor.Red,
                    LoggerLevel.Fatal => ConsoleColor.Magenta,
                    _ => ConsoleColor.White
                };
                Console.WriteLine($"[{o.GetType().Name}/{level}] {s}");
                Console.ResetColor();
            }
        }

        public static void Log(LoggerLevel level, string s)
        {
            lock (Console.Out)
            {
                Console.ForegroundColor = level switch
                {
                    LoggerLevel.Debug => ConsoleColor.White,
                    LoggerLevel.Info => ConsoleColor.Green,
                    LoggerLevel.Warn => ConsoleColor.Yellow,
                    LoggerLevel.Error => ConsoleColor.Red,
                    LoggerLevel.Fatal => ConsoleColor.Magenta,
                    _ => ConsoleColor.White
                };
                Console.WriteLine($"[{new StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}/{level}] {s}");
                Console.ResetColor();
            }
        }

        public static string GetHttpContent(string uri)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5);
                    return client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result;
                }
            }
            catch
            {
                return null;
            }
        }

        public static async Task<JObject> GetHttp(string uri)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = new TimeSpan(0, 0, 5);
                    return JObject.Parse(await (await client.GetAsync(uri)).Content.ReadAsStringAsync());
                }
            }
            catch
            {
                return null;
            }
        }

        public static T ParseTo<T>(this string str)
        {
            if (typeof(T) == typeof(string))
                return (T)(object)str;
            else
                return (T) typeof(T).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)
                    .Invoke(null, new object[] { str });
        }
    }
}
