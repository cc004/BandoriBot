using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BandoriBot.Services
{
    public class Character
    {
        public int type, level;
        public bool weaponed;
    }

    public class Team
    {
        public Character[] atts, defs;
        public int likes, dislikes, id;
    }

    public sealed class JJCManager : IDisposable
    {
        public static JJCManager Instance = new JJCManager(Path.Combine("jjc"));

#pragma warning disable CS0649
        [JsonObject]
        private class Character
        {
            public int id, position;
            public string name, avatar;
        }

        private readonly List<Character> characters;
        private readonly Dictionary<string, Image> textures;
        private readonly Dictionary<string, int> nicknames;
        private readonly Font font;

        public JJCManager(string root)
        {
            textures = new Dictionary<string, Image>();
            nicknames = new Dictionary<string, int>();
            characters = JsonConvert.DeserializeObject<List<Character>>(File.ReadAllText(Path.Combine(root, "list.json")));
            font = new Font(FontFamily.GenericMonospace, 15);

            foreach (var file in Directory.GetFiles(root))
                if (file.EndsWith(".png"))
                    textures.Add(Path.GetFileNameWithoutExtension(file), Image.FromFile(file));
            foreach (var line in File.ReadAllText(Path.Combine(root, "nickname.csv")).Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                var splits = line.Split(',');
                var id = characters.SingleOrDefault(c => c.name.EndsWith(splits[2]));
                if (id == null)
                {
                    continue;
                }
                foreach (var nickname in splits.Skip(2))
                    nicknames.Add(nickname, id.id);
            }
        }

        private void DrawToGraphics(Graphics canvas, Services.Character c, int offx, int offy)
        {
            canvas.DrawImage(textures[c.type.ToString()], new Rectangle(offx, offy, 100, 100));
            canvas.DrawImage(textures["search-li"], new Rectangle(offx, offy, 100, 100));
            for (int i = 0; i < c.level; ++i)
                canvas.DrawImage(textures["star"], new Rectangle(5 + 12 * i + offx, 80 + offy, 15, 15));
            if (c.weaponed) canvas.DrawImage(textures["arms"], new Rectangle(15 + offx, 15 + offy, 15, 15));
        }

        private void DrawToGraphics(Graphics canvas, Team t, int offx, int offy)
        {
            for (int i = 0; i < 5; ++i)
            {
                DrawToGraphics(canvas, t.atts[i], 110 * i + offx, offy);
                DrawToGraphics(canvas, t.defs[i], 110 * i + 590 + offx, offy);
            }
            canvas.DrawString($"顶：{t.likes} 踩：{t.dislikes} ", font, Brushes.Black, offx, offy + 100);
        }
        
        public Image GetImage(Team[] teams)
        {
            var n = teams.Length;
            var result = new Bitmap(1130, 120 * n + 20);
            var canvas = Graphics.FromImage(result);
            canvas.Clear(Color.White);
            for (int i = 0; i < n; ++i) DrawToGraphics(canvas, teams[i], 0, i * 120);
            canvas.DrawString($"powered by www.bigfun.cn", font, Brushes.Black, 0, 120 * n);
            canvas.Dispose();
            return result;
        }

        public Team[] Callapi(string[] defs)
        {
            int[] ids = new int[5];
            int i = 0;
            try
            {
                for (; i < 5; ++i) ids[i] = nicknames[defs[i]];
            }
            catch (KeyNotFoundException)
            {
                throw new Exception($"未能识别{defs[i]}");
            }
            JObject obj;
            using (var client = new HttpClient())
            {
                obj = JObject.Parse(client.GetStringAsync("https://www.bigfun.cn/api/client/web?method=findLineUp&order=like&roles=" + string.Join(",", ids)).Result);
            }

            Func<JToken, Services.Character > parser = token => new Services.Character
            {
                level = (int)token["level"],
                weaponed = (bool)token["is_have_weapon"],
                type = (int)token["role_id"]
            };

            return obj["data"].Select(token => new Team
            {
                id = (int)token["id"],
                likes = (int)token["like"],
                dislikes = (int)token["dislike"],
                atts = token["win"].Select(parser).ToArray(),
                defs = token["lose"].Select(parser).ToArray()
            }).ToArray();
        }

        public void Dispose()
        {
            foreach (var pair in textures) pair.Value.Dispose();
        }
    }
}
