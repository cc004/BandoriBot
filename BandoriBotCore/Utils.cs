using BandoriBot.Config;
using BandoriBot.Handler;
using BandoriBot.Models;
using Newtonsoft.Json.Linq;
using Sora.Entities;
using Sora.Entities.Base;
using Sora.Entities.CQCodes;
using Sora.Entities.CQCodes.CQCodeModel;
using Sora.Enumeration;
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
using Native.Csharp.App.Terraria;
using Sora.Enumeration.EventParamsType;
using Image = System.Drawing.Image;

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

        public static async Task<List<GroupMemberInfo>> GetMemberList(this SoraApi session, long groupId)
        {
            return (await session.GetGroupMemberList(groupId)).groupMemberList
                .Select(info => new GroupMemberInfo
                {
                    GroupId = groupId,
                    QQId = info.UserId,
                    PermitType = info.Role switch
                    {
                        MemberRoleType.Owner => PermitType.Holder,
                        MemberRoleType.Admin => PermitType.Manage,
                        _ => PermitType.None
                    }
                }).ToList();
        }

        public static async Task<List<Models.GroupInfo>> GetGroupList0(this SoraApi session)
        {
            return (await session.GetGroupList())
                .groupList.Select(info => new Models.GroupInfo
                {
                    Id = info.GroupId,
                    Name = info.GroupName
                }).ToList();
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

        public static List<CQCode> GetMessageChain(string msg)
        {
            Match match;
            List<CQCode> result = new List<CQCode>();

            while ((match = codeReg.Match(msg)).Success)
            {
                if (!string.IsNullOrEmpty(match.Groups[1].Value))
                    result.Add(CQCode.CQText(match.Groups[1].Value.Decode()));
                var val = match.Groups[3].Value;
                switch (match.Groups[2].Value)
                {
                    case "mirai:at": result.Add(CQCode.CQAt(long.Parse(val))); break;
                    case "mirai:imageid": result.Add(CQCode.CQImage(val.Decode().FixImage(), false)); break;
                    case "mirai:imageurl": result.Add(CQCode.CQImage(val.Decode())); break;
                    case "mirai:imagepath": result.Add(CQCode.CQImage(val.Decode())); break;
                    case "mirai:imagenew": result.Add(CQCode.CQImage(val.Decode())); break;
                    case "mirai:atall": result.Add(CQCode.CQAtAll()); break;
                    case "mirai:json": result.Add(CQCode.CQJson(val.Decode())); break;
                    case "mirai:xml": result.Add(CQCode.CQXml(val.Decode())); break;
                    case "mirai:poke": result.Add(CQCode.CQPoke(long.Parse(val))); break;
                    case "mirai:face": result.Add(CQCode.CQFace(int.Parse(val))); break;
                    case "CQ:at,qq": result.Add(CQCode.CQAt(long.Parse(val))); break;
                    case "CQ:face,id": result.Add(CQCode.CQFace(int.Parse(val))); break;
                    default: result.Add(CQCode.CQText($"[{match.Groups[2].Value}={match.Groups[3].Value}]")); break;
                }
                msg = match.Groups[4].Value;
            }

            if (!string.IsNullOrEmpty(msg)) result.Add(CQCode.CQText(msg.Decode()));

            return result.ToList();
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
        public static bool PlayerOnline(string name)
        {
            return GetAllOnlinePlayers().Contains(name);
        }
        public static string GetOnlineServer(string name)
        {
            if (PlayerOnline(name))
                foreach (var s in Configuration.GetConfig<ServerManager>().servers)
                {
                    if (GetOnlinePlayers(s.Value).Contains(name))
                        return s.Key;
                }
            else
                return null;
            return null;
        }
        public static int GetItemStack(Server server,string name,int id)
        {
            return int.Parse (server.RunRest("/v1/itemrank/rankboard?&id=" + id).Where(t => t["name"].ToString() == name).FirstOrDefault()["count"].ToString());
        }
        public static string GetMoney(Server server,string name)
        {
            int copper = GetItemStack(server,name ,71), 
                silver = GetItemStack(server, name, 72), 
                gold = GetItemStack(server, name, 73), 
                platinum = GetItemStack(server, name, 74);
            string res = "";
            if (platinum != 0)
            {
                res += platinum + "铂金";
            }
            if(gold != 0)
            {
                res += gold + "金";
            }
            if (silver != 0)
            {
                res += silver + "银";
            }
            if(copper != 0)
            {
                res += copper + "铜";
            }
            if (res == "") res = "无产阶级";
            return res;
        }
        public static string[] GetOnlinePlayers(Server server)
        {
            return server.RunRest("/v2/users/activelist")["activeusers"]
                .ToString().Split('\t').Where((s) => !string.IsNullOrWhiteSpace(s)).ToArray();
        }
        public static string[] GetAllOnlinePlayers()
        {
            var server = Configuration.GetConfig<ServerManager>().servers["流光之城"];
            var online = string.Join("\n", server.RunCommand("/list")["response"].Select(s => s.ToString()));
            return online.Split('：')[1].Split(',');
        }
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
            return string.Concat(chain.MessageList.Select(msg => GetCQMessage(msg)));
        }

        public static string Encode(this string str)
        {
            return str.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;");
        }

        public static string Decode(this string str)
        {
            return str.Replace("&#91;", "[").Replace("&#93;", "]").Replace("&amp;", "&");
        }

        private static string GetCQMessage(CQCode msg)
        {
            switch (msg.CQData)
            {
                case Face face:
                    return $"[mirai:face={face.Id}]";
                case Text plain:
                    return plain.Content.Encode();
                case At at:
                    return $"[mirai:at={at.Traget}]";
                case Sora.Entities.CQCodes.CQCodeModel.Image img:
                    return $"[mirai:imagenew={img.ImgFile}]";
                case Poke poke:
                    return $"[mirai:poke={poke.Uid}]";
                case Code code:
                    switch (msg.Function)
                    {
                        case CQFunction.Json: return $"[mirai:json={code.Content}]";
                        case CQFunction.Xml: return $"[mirai:xml={code.Content}]";
                        default: return "";
                    }
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
