using BandoriBot.DataStructures;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BandoriBot.Services
{
    [JsonObject]
    public class Rate
    {
        public int weightTotal;
        public byte rarityIndex;
        public float rate;
    }

    [JsonObject]
    public class Card
    {
        public int weight;
        public short situationId;
        public byte rarityIndex;
    }

    [JsonObject]
    public class Behavior
    {
        public byte count;
        public string behavior;
    }

    [JsonObject]
    public class Detail
    {
    }

    [JsonObject]
    public class GachaInfo
    {
        public string gachaName;
        public Behavior[] paymentMethods;
        public Card[] details;
        public Rate[] rates;
        public long publishedAt;
        public short gachaId;
    }
    [JsonObject]
    public class CardInfo
    {
        public string cardRes;
        public Attr attr;
        public int characterId;
        public byte rarity;
    }

    public class GachaManager
    {
        public static GachaManager Instance = new GachaManager(Path.Combine("gacha"));
        private readonly Dictionary<string, string> Cards;
        private readonly string[] Attrs;
        private readonly string[] Bands;
        private readonly string[] Frames;
        private readonly string[] Miscs;
        private readonly Dictionary<string, GachaInfo> GachaMap;
        private readonly Dictionary<string, CardInfo> CardMap;
        private readonly GachaInfo[] Gachas;

        private string GetFrame(Attr attribute, byte rarity)
        {
            return rarity == 0 ? Frames[(int)attribute] : Frames[rarity + 2];
        }
        private string GetCard(string res) => Cards.ContainsKey(res) ? Cards[res] : null;
        private string GetAttribute(Attr attribute) => Attrs[(int)attribute];
        private string GetBands(byte band) => Bands[band];
        private string GetMisc(int index) => Miscs[index];

        public GachaInfo[] GetGachas()
        {
            return Gachas;
        }

        public GachaManager(string rootPath)
        {
            Cards = new Dictionary<string, string>();
            var cardpath = Path.Combine(rootPath, "card");
            foreach (var file in Directory.GetFiles(cardpath))
                Cards.Add(Path.GetFileNameWithoutExtension(file), file);

            Frames = new string[7];
            Frames[4] = Path.Combine(rootPath, "common", "frame-rare.png");
            Frames[5] = Path.Combine(rootPath, "common", "frame-sr.png");
            Frames[6] = Path.Combine(rootPath, "common", "frame-ssr.png");

            Attrs = new string[4];
            foreach (var attr in Enum.GetNames(typeof(Attr)))
            {
                var index = (int)Enum.Parse(typeof(Attr), attr);
                Attrs[index] = Path.Combine(rootPath, "common", $"attr-{attr}.png");
                Frames[index] = Path.Combine(rootPath, "common", $"frame-normal-{attr}.png");
            }

            Bands = new string[5];
            for (int i = 0; i < 5; ++i)
                Bands[i] = Path.Combine(rootPath, "common", $"band-{i + 1}.png");

            Miscs = new string[3];
            Miscs[0] = Path.Combine(rootPath, "common", "rarity-0.png");
            Miscs[1] = Path.Combine(rootPath, "common", "rarity-1.png");
            Miscs[2] = Path.Combine(rootPath, "common", "background.png");

            var json = JObject.Parse(File.ReadAllText(Path.Combine(rootPath, "MasterDB_cn.json")));
            GachaMap = json["gachaMap"]["entries"].ToObject<Dictionary<string, GachaInfo>>();
            CardMap = json["cardInfos"]["entries"].ToObject<Dictionary<string, CardInfo>>();

            Gachas = GachaMap
                .OrderByDescending(prop => prop.Value.publishedAt)
                .Select((prop) => prop.Value).ToArray();
        }

        private Image LoadTexture(string path) => Image.FromFile(path);

        public Image GenerateCard(short cardId, bool transformed)
        {
            var img = new Bitmap(180, 180);
            var cardInfo = CardMap[cardId.ToString()];
            var resource = cardInfo.cardRes;
            var attribute = cardInfo.attr;
            var rarity = cardInfo.rarity;
            var band = (byte)((cardInfo.characterId - 1) / 5);

            var tex = LoadTexture(GetCard(resource + (transformed ? "_after_training" : "_normal")) ??
                      GetCard(resource + "_normal"));
            var frame = LoadTexture(GetFrame(attribute, rarity));
            var star = LoadTexture(GetMisc(transformed ? 1 : 0));
            var bandtex = LoadTexture(GetBands(band));
            var attr = LoadTexture(GetAttribute(attribute));

            var canvas = Graphics.FromImage(img);
            canvas.Clear(Color.Transparent);
            canvas.DrawImage(tex, 0, 0);
            canvas.DrawImage(frame, 0, 0);
            canvas.DrawImage(bandtex, new RectangleF(2, 2, bandtex.Width * 1f, bandtex.Height * 1f));
            canvas.DrawImage(attr, new RectangleF(132, 2, 46, 46));

            for (int i = 0; i < rarity; ++i)
                canvas.DrawImage(star, new Rectangle(0, 170 - 28 * (i + 1), 35, 35));

            img.MakeTransparent();

            tex.Dispose(); frame.Dispose(); star.Dispose(); bandtex.Dispose(); attr.Dispose();

            return img;
        }

        public async Task<Tuple<string, Image>> Gacha(short gachaId, Random rand = null)
        {
            await Task.Yield();

            var gachaInfo = GachaMap[gachaId.ToString()];
            var detail = gachaInfo.details;
            rand ??= new Random();
            var method = gachaInfo.paymentMethods.OrderByDescending((obj) => (int)obj.count).First();
            var behavior = method.behavior;
            var count = method.count;
            short[] cardIds = new short[count];

            for (int i = 0; i < count; ++i)
            {
                var rate = rand.NextDouble() * 100f;
                Rate rarityIndex = null, rarity3 = null, rarity4 = null;
                foreach (var obj in gachaInfo.rates)
                {
                    if (obj.rarityIndex == 3)
                        rarity3 = obj;
                    if (obj.rarityIndex == 4)
                        rarity4 = obj;
                    rate -= obj.rate;
                    if (rate < 0f && rarityIndex == null)
                    {
                        rarityIndex = obj;
                    }
                }

                if (i == count - 1)
                {
                    switch (behavior)
                    {
                        case "fixed_4_star_once":
                            rarityIndex = rarity4;
                            break;
                        case "over_the_3_star_once":
                            if (rarityIndex.rarityIndex == 2)
                                rarityIndex = rarity3;
                            break;
                    }
                }

                byte index = rarityIndex.rarityIndex;
                var rweight = rand.Next(rarityIndex.weightTotal);

                foreach (var card in detail)
                {
                    if (card.rarityIndex != index)
                        continue;
                    rweight -= card.weight;
                    if (rweight < 0)
                    {
                        cardIds[i] = card.situationId;
                        break;
                    }
                }
            }

            var result = new Bitmap(GetMisc(2));
            var canvas = Graphics.FromImage(result);

            for (int i = 0; i < count; ++i)
            {
                using var card = GenerateCard(cardIds[i], false);
                canvas.DrawImage(card, new Rectangle(292 + 324 * (i % 5), 496 + 326 * (i / 5), 290, 290));
            }

            return new Tuple<string, Image>(gachaInfo.gachaName, result);
        }
    }
}
