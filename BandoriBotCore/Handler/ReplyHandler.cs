using BandoriBot.Commands;
using BandoriBot.Config;
using BandoriBot.DataStructures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot.Handler
{
    using DataType = Dictionary<string, List<Reply>>;
    public class ReplyHandler : SerializableConfiguration<List<DataType>>, IMessageHandler
    {
        private const int version = 2;
        public override string Name => "reply.json";
        public DataType data, data2, data3;

        public DataType this[int index] =>
            index switch
            {
                1 => data,
                2 => data2,
                3 => data3,
                _ => null
            };

        public override void LoadDefault()
        {
            data = new DataType();
            data2 = new DataType();
            data3 = new DataType();
        }

        public override void LoadFrom(BinaryReader br)
        {
            base.LoadFrom(br);
            data = t[0];
            data2 = t[1];
            data3 = t[2];
        }

        public override void SaveTo(BinaryWriter bw)
        {
            t = new List<DataType>
            {
                data,
                data2,
                data3
            };
            base.SaveTo(bw);
        }

        public bool OnMessage(string message, Source Sender, bool isAdmin, ResponseCallback callback)
        {
            var raw = Utils.FindAtMe(message, out var isme, Sender.Session.QQNumber ?? 0).Trim();
            if (isme)
            {
                if (GetConfig<Blacklist>().hash.Contains(Sender.FromQQ))
                {
                    callback("调戏机器人吃枣药丸");
                    return true;
                }

                var pending = data.TryGetValue(raw, out var list) ?
                    list.Select((r) => r.reply).ToList() :
                    new List<string>();

                foreach (var pair in data2)
                {
                    var match = new Regex(@$"^{Utils.FixRegex(pair.Key)}$").Match(raw);
                    if (match.Success)
                    {
                        var reply = pair.Value[new Random().Next(pair.Value.Count)].reply;
                        pending.Add(new Regex(@"\$.").Replace(reply, (m) =>
                        {
                            var c = m.Value[1];
                            if (c >= '0' && c <= '9')
                            {
                                var n = (int)(c - '0');
                                if (n < match.Groups.Count)
                                    return match.Groups[n].Value;
                            }
                            else if (c == '$') return "$";
                            else if (c == 'g') return Sender.FromGroup.ToString();
                            else if (c == 'q') return Sender.FromQQ.ToString();
                            return m.Value;
                        }));
                    }
                }
                if (pending.Count > 0)
                    callback(pending[new Random().Next(pending.Count)]);

                return true;
            }
            else
            {
                var pending = new List<string>();
                foreach (var pair in data3)
                {
                    var match = new Regex(@$"^{Utils.FixRegex(pair.Key)}$").Match(raw);
                    if (match.Success)
                    {
                        var reply = pair.Value[new Random().Next(pair.Value.Count)].reply;
                        pending.Add(new Regex(@"\$.").Replace(reply, (m) =>
                        {
                            var c = m.Value[1];
                            if (c >= '0' && c <= '9')
                            {
                                var n = (int)(c - '0');
                                if (n < match.Groups.Count)
                                    return match.Groups[n].Value;
                            }
                            else if (c == '$') return "$";
                            else if (c == 'g') return Sender.FromGroup.ToString();
                            else if (c == 'q') return Sender.FromQQ.ToString();
                            return m.Value;
                        }));
                    }
                }

                if (pending.Count > 0)
                {
                    callback(pending[new Random().Next(pending.Count)]);
                    return true;
                }
                return false;
            }
        }
    }
}
