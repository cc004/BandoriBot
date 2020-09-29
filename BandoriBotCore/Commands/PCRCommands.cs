using BandoriBot.Config;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCRClient;
using PCRClientTest;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot.Commands
{
    public class CCDCommand : ICommand
    {
        public List<string> Alias => new List<string> { "查出刀" };

        public void Run(CommandArgs args)
        {
            string[] splits = args.Arg.Trim().Split('-');
            DateTime start = DateTime.ParseExact(splits[0], "MMddHH", CultureInfo.CurrentCulture);
            DateTime end = DateTime.ParseExact(splits[1], "MMddHH", CultureInfo.CurrentCulture);
            args.Callback(Configuration.GetConfig<PCRConfig>().Query(start, end, qq => args.Source.Session.GetName(args.Source.FromGroup, qq)));
        }
    }

    public class SCCommand : ICommand
    {
        public List<string> Alias => new List<string> { "生成csv" };

        public void Run(CommandArgs args)
        {
            string[] splits = args.Arg.Trim().Split('-');
            DateTime start = DateTime.ParseExact(splits[0], "MMddHH", CultureInfo.CurrentCulture);
            DateTime end = DateTime.ParseExact(splits[1], "MMddHH", CultureInfo.CurrentCulture);
            var name = args.Arg.Trim() + ".csv";
            File.WriteAllText(name, Configuration.GetConfig<PCRConfig>().GetCSV(start, end, qq => args.Source.Session.GetName(args.Source.FromGroup, qq)), Encoding.UTF8);

            args.Callback($"数据已保存到{args.Arg.Trim() + ".csv"}");
        }
    }

    public class SLCommand : ICommand
    {
        public List<string> Alias => new List<string> { "私聊提醒" };

        public void Run(CommandArgs args)
        {
            var split1 = args.Arg.Trim().Split(' ');
            string[] splits = split1[0].Split('-');
            var result = "";
            DateTime start = DateTime.ParseExact(splits[0], "MMddHH", CultureInfo.CurrentCulture);
            DateTime end = DateTime.ParseExact(splits[1], "MMddHH", CultureInfo.CurrentCulture);
            var list = Configuration.GetConfig<PCRConfig>().Query(start, end);
            foreach (var info in Common.CqApi.GetMemberList(args.Source.FromGroup).Where(info => !list.Contains(info.QQId)))
            {
                //Common.CqApi.SendPrivateMessage(info.QQId, string.Join(" ", split1.Skip(1)));
                result += $"[mirai:at={info.QQId}] ";
            }

            args.Callback("已私聊提醒下列未出刀的屑：\n" + result);
        }
    }

    public class TBCommand : ICommand
    {
        public List<string> Alias => new List<string> { "同步刀" };

        public void Run(CommandArgs args)
        {
            if (!args.IsAdmin) return;
            string[] splits = args.Arg.Trim().Split(' ');
            Configuration.GetConfig<PCRConfig>().SetData(int.Parse(splits[0]), int.Parse(splits[1]), int.Parse(splits[2]));
        }
    }

    public class DDCommand : ICommand
    {
        public List<string> Alias => new List<string> { "代刀" };

        public void Run(CommandArgs args)
        {
            var match = new Regex(@"^\[mirai:at=(.*?)\] (.*)$").Match(args.Arg.Trim());

            if (!match.Success)
            {
                args.Callback("代刀 @xxx 伤害");
                return;
            }

            int damage;
            long qq = long.Parse(match.Groups[1].Value);

            try
            {
                damage = int.Parse(match.Groups[2].Value);
                if (damage < 0)
                {
                    args.Callback("伤害不能是负数!");
                    return;
                }
            }
            catch
            {
                args.Callback("格式错误，请输入数字！");
                return;
            }

            try
            {
                args.Callback(string.Format(Configuration.GetConfig<PCRConfig>().Add(qq, damage),
                    args.Source.Session.GetName(args.Source.FromGroup, qq)));
            }
            catch (Exception e)
            {
                args.Callback(e.Message);
                return;
            }
        }
    }


    public class CDCommand : ICommand
    {
        public List<string> Alias => new List<string> { "出刀" };

        public void Run(CommandArgs args)
        {
            int damage;
            try
            {
                damage = int.Parse(args.Arg.Trim());
                if (damage < 0)
                {
                    args.Callback("伤害不能是负数!");
                    return;
                }
            }
            catch
            {
                args.Callback("格式错误，请输入数字！");
                return;
            }

            try
            {
                args.Callback(string.Format(Configuration.GetConfig<PCRConfig>().Add(args.Source.FromQQ, damage), 
                    args.Source.Session.GetName(args.Source.FromGroup, args.Source.FromQQ)));
            }
            catch (Exception e)
            {
                args.Callback(e.Message);
                return;
            }
        }
    }

    public class PCRRunCommand : ICommand
    {
        public List<string> Alias => new List<string> { "/pcr" };

        public void Run(CommandArgs args)
        {
            var trimed = args.Arg.Trim();
            var sp = trimed.IndexOf(" ");
            try
            {
                args.Callback(PCRManager.Instance.client.Callapi(trimed.Substring(0, sp), JObject.Parse(trimed.Substring(sp))).ToString(Formatting.Indented));
            }
            catch (ApiException e)
            {
                args.Callback(e.Message);
                PCRManager.Instance.Do_Login();
            }
        }
    }

    public class RCCommand : ICommand
    {
        private static List<Color> colors = new List<Color>
        {
            Color.Red, Color.Orange, Color.Yellow, Color.LimeGreen, Color.LightSeaGreen, Color.Cyan, Color.Blue,
            Color.MediumPurple, Color.Pink, Color.Gray, Color.IndianRed, Color.OrangeRed, Color.ForestGreen,
            Color.DarkCyan, Color.LightSkyBlue, Color.Violet, Color.DeepPink, Color.DarkRed,
            Color.Aquamarine, Color.DarkBlue, Color.DarkMagenta, Color.LightSlateGray
        };

        private static Random random = new Random();

        private static Color RandomColor => Color.FromArgb(random.Next(256), random.Next(256), random.Next(256));

        private static Color ColorHue(int id)
        {
            while (id >= colors.Count)
                colors.Add(RandomColor);
            return colors[id];
        }

        public List<string> Alias => new List<string> { "日程" };

        public void Run(CommandArgs args)
        {
            if (!string.IsNullOrEmpty(args.Arg)) return;
            var js = Utils.GetHttpContent("https://static.biligame.com/pcr/gw/calendar.js");
            var start = js.IndexOf("var data = ") + 11;
            var end = js.IndexOf("window.__calendar = data;");
            var json = JArray.Parse(js.Substring(start, end - start));
            var now = DateTime.Now.AddHours(-5);
            var alist = new List<string>();
            var datelist = new Dictionary<string, HashSet<int>>();

            //analyze
            int i;

            for (i = 0; i < 21; ++i)
            {
                var date = now.AddDays(i);
                var data = json.SingleOrDefault(token =>
                    int.Parse(token.Value<string>("year")) == date.Year &&
                    int.Parse(token.Value<string>("month")) == date.Month)
                    ?.Value<JObject>("day")
                    ?.Value<JObject>(date.Day.ToString());

                if (data == null) break;

                var set = new HashSet<int>();
                foreach (var prop in data.Properties())
                {
                    var regex2 = new Regex("<div class='cl-t'>(.*?)</div><div class='cl-d'>(.*?)</div>");

                    foreach (Match match in regex2.Matches(prop.Value.Value<string>()))
                    {
                        var val = $"{match.Groups[1].Value}";
                        var id = alist.IndexOf(val);

                        if (id == -1)
                        {
                            id = alist.Count;
                            alist.Add(val);
                        }

                        set.Add(id);
                    }
                }

                datelist.Add(date.ToShortDateString(), set);
            }

            //draw
            //border 10px, color 20px, date 80x, desc 200px

            const int wborder = 10, wsquare = 20, wdesc = 200, wdate = 80;
            var width = wborder + wdate + wsquare * alist.Count + wborder + wsquare + wdesc + wborder;
            var height = wborder + Math.Max(datelist.Count * wsquare, alist.Count * wsquare) + wborder;
            var font = new Font(FontFamily.GenericMonospace, 8);

            var img = new Bitmap(width, height);
            var canvas = Graphics.FromImage(img);

            canvas.Clear(Color.White);

            //col lines and alist descriptions
            int m = alist.Count, n = datelist.Count;
            var descStart = wborder + wdate + wsquare * alist.Count + wborder;

            i = 0;
            foreach (var pair in datelist)
            {
                canvas.DrawString(pair.Key, font, Brushes.Black, wborder, wborder + i * wsquare + 6);

                foreach (var id in pair.Value)
                    canvas.FillRectangle(new SolidBrush(ColorHue(id)), new Rectangle(wborder + wdate + wsquare * id, wborder + i * wsquare, wsquare, wsquare));

                canvas.DrawLine(Pens.Black, wborder, wborder + i * wsquare, wborder + wdate + m * wsquare, wborder + i * wsquare);
                ++i;
            }
            canvas.DrawLine(Pens.Black, wborder, wborder + i * wsquare, wborder + wdate + m * wsquare, wborder + i * wsquare);

            canvas.DrawLine(Pens.Black, wborder, wborder, wborder, wborder + n * wsquare);
            for (i = 0; i < m; ++i)
            {
                canvas.FillRectangle(new SolidBrush(ColorHue(i)), new Rectangle(descStart, wborder + i * wsquare, wsquare, wsquare));
                canvas.DrawRectangle(Pens.Black, new Rectangle(descStart, wborder + i * wsquare, wsquare, wsquare));
                canvas.DrawString(alist[i], font, Brushes.Black, descStart + wsquare, wborder + i * wsquare + 6);
                canvas.DrawLine(Pens.Black, wborder + wdate + i * wsquare, wborder, wborder + wdate + i * wsquare, wborder + n * wsquare);
            }
            canvas.DrawLine(Pens.Black, wborder + wdate + i * wsquare, wborder, wborder + wdate + i * wsquare, wborder + n * wsquare);


            canvas.Dispose();

            args.Callback(Utils.GetImageCode(img));

            img.Dispose();
        }
    }
    public class CPMCommand : ICommand
    {
        public List<string> Alias => new List<string> { "查排名" };

        public void Run(CommandArgs args)
        {
            args.Callback(PCRManager.Instance.GetRankStatistic(int.Parse(args.Arg.Trim())));
        }
    }
}
