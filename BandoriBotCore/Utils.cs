using BandoriBot.Services;
using BandoriBot.Models;
using Newtonsoft.Json.Linq;
using Sora.Entities;
using Sora.Entities.Base;
using Sora.Enumeration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration.EventParamsType;
using Image = System.Drawing.Image;

namespace BandoriBot
{
    public static class Utils
    {

        public static string FindAtMe(string origin, out bool isat)
        {
            isat = false;
            foreach (var qq in MessageHandler.selfids)
            {
                var at = $"[mirai:at={qq}]";
                isat |= origin.Contains(at);
                origin = origin.Replace(at, "");
            }

            return origin;
        }

        public static int SetGroupSpecialTitle(this SoraApi session, long groupId, long qqId, string specialTitle, TimeSpan time)
        {
            throw new NotImplementedException();
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

        private static string FixImage(this string imgcode)
        {
            return imgcode[1..37].Replace("-", "").ToLower() + ".image";
        }

        public static MessageBody GetMessageChain(string msg)
        {
            Match match;
            List<SoraSegment> result = new List<SoraSegment>();

            while ((match = codeReg.Match(msg)).Success)
            {
                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                    result.Add(SoraSegment.Text(match.Groups[1].Value.Decode()));
                var val = match.Groups[3].Value;
                switch (match.Groups[2].Value)
                {
                    case "mirai:at": result.Add(SoraSegment.At(long.Parse(val))); break;
                    case "mirai:imageid": result.Add(SoraSegment.Image(val.Decode().FixImage(), false)); break;
                    case "mirai:imageurl": result.Add(SoraSegment.Image(val.Decode(), false)); break;
                    case "mirai:imagepath": result.Add(SoraSegment.Image(Path.GetFullPath(val.Decode()), false)); break;
                    case "mirai:imagenew": result.Add(SoraSegment.Image(val.Decode(), false)); break;
                    case "mirai:atall": result.Add(SoraSegment.AtAll()); break;
                    case "mirai:json": result.Add(SoraSegment.Json(val.Decode())); break;
                    case "mirai:xml": result.Add(SoraSegment.Xml(val.Decode())); break;
                    case "mirai:poke": result.Add(SoraSegment.Poke(long.Parse(val))); break;
                    case "mirai:face": result.Add(SoraSegment.Face(int.Parse(val))); break;
                    case "CQ:at,qq": result.Add(SoraSegment.At(long.Parse(val))); break;
                    case "CQ:face,id": result.Add(SoraSegment.Face(int.Parse(val))); break;
                    case "mirai:record": result.Add(SoraSegment.Record(val.Decode())); break;
                    default: result.Add(SoraSegment.Text($"[{match.Groups[2].Value}={match.Groups[3].Value}]")); break;
                }
                msg = match.Groups[4].Value;
            }

            if (!string.IsNullOrEmpty(msg)) result.Add(SoraSegment.Text(msg.Decode()));

            return new MessageBody(result);
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

        public static async Task<string> GetName(this SoraApi session, long group, long qq)
        {
            try
            {
                return (await session.GetGroupMemberInfo(qq, group)).memberInfo.Card;
            }
            catch (Exception e)
            {
                Log(LoggerLevel.Error, e.ToString());
                return qq.ToString();
            }
        }

        public static async Task<string> GetName(this Source source)
            => await source.Session.GetName(source.FromGroup, source.FromQQ);

        internal static string GetCQMessage(Message chain)
        {
            return string.Concat(chain.MessageBody.Select(GetCQMessage));
        }

        public static string Encode(this string str)
        {
            return str.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;");
        }

        public static string Decode(this string str)
        {
            return str.Replace("&#91;", "[").Replace("&#93;", "]").Replace("&amp;", "&");
        }

        private static string GetCQMessage(SoraSegment msg)
        {
            switch (msg.Data)
            {
                case FaceSegment face:
                    return $"[mirai:face={face.Id}]";
                case TextSegment plain:
                    return plain.Content.Encode();
                case AtSegment at:
                    return $"[mirai:at={at.Target}]";
                case ImageSegment img:
                    return $"[mirai:imagenew={img.ImgFile.Encode()}]";
                case PokeSegment poke:
                    return $"[mirai:poke={poke.Uid}]";
                case CodeSegment code:
                    switch (msg.MessageType)
                    {
                        case SegmentType.Json: return $"[mirai:json={code.Content.Encode()}]";
                        case SegmentType.Xml: return $"[mirai:xml={code.Content.Encode()}]";
                        default: return "";
                    }
                case RecordSegment record:
                    return $"[mirai:record={record.RecordFile.Encode()}]";
                default:
                    return "";//msg.ToString().Encode();
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
            return $"[mirai:imagepath={Path.GetFullPath(path)}]";
        }

        public static string ToCache(this byte[] b)
        {
            var path = Path.Combine("imagecache", $"cache{new Random().Next()}");
            File.WriteAllBytes(path, b);
            return Path.GetFullPath(path);
        }

        private static Font font = new Font(FontFamily.GenericMonospace, 10f, FontStyle.Regular);
        private static Brush brush = Brushes.Black;
        public static string ToImageText(this string str)
        {
            using var bitmap = new Bitmap(1, 1);
            using var g = Graphics.FromImage(bitmap);
            var lines = str.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var sizes = lines.Select(l => g.MeasureString(l, font)).ToArray();
            var img = new Bitmap((int)sizes.Max(s => s.Width) + 6, (int)sizes.Sum(s => s.Height) + 6);
            using (var g2 = Graphics.FromImage(img))
            {
                g2.Clear(Color.White);
                var h = 3f;
                for (int i = 0; i < lines.Length; ++i)
                {
                    g2.DrawString(lines[i], font, brush, 3, h);
                    h += sizes[i].Height;
                }
            }

            return GetImageCode(img);
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
                var now = DateTime.Now;
                var text = $"[{now:HH:mm:ss}] [{o.GetType().Name}/{level}] {s}";
                Console.WriteLine(text);
                Console.ResetColor();
                File.AppendAllText($"Data\\{now:yyyy-MM-dd}.log", text + "\n");
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
                var now = DateTime.Now;
                var text = $"[{now:HH:mm:ss}] [{new StackTrace().GetFrame(1).GetMethod().DeclaringType.Name}/{level}] {s}";
                Console.WriteLine(text);
                Console.ResetColor();
                File.AppendAllText($"Data\\{now:yyyy-MM-dd}.log", text + "\n");
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
                    client.Timeout = new TimeSpan(0, 0, 10);
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
                return (T)typeof(T).GetMethod("Parse", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(string) }, null)
                    .Invoke(null, new object[] { str });
        }

        private static DateTime dateTimeStart = TimeZoneInfo.ConvertTime(new DateTime(1970, 1, 1), TimeZoneInfo.Local);

        // 时间戳转为C#格式时间
        public static DateTime ToDateTime(this long timeStamp)
        {
            return dateTimeStart.Add(new TimeSpan(10000 * timeStamp));
        }

        // DateTime时间格式转换为Unix时间戳格式
        public static long ToTimestamp(this DateTime time)
        {
            return (long)(time - dateTimeStart).TotalMilliseconds;
        }
    }
}
