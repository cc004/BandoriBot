using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SekaiClient.Datas
{
    [JsonObject]
    public class Event
    {
        public int id;
        public long startAt, aggregateAt;
    }

    [JsonObject]
    public class MusicDifficulty
    {
        public string musicDifficulty;
        public int noteCount, id;
    }
    [JsonObject]
    public class MusicVocal
    {
        public int id;
        public int musicId;
    }
    [JsonObject]
    public class Music
    {
        public int id;
    }

    [JsonObject]
    public class Card
    {
        public int characterId, rarity, skillId, id;
        public string prefix, attr;
    }

    [JsonObject]
    public class GameCharacter
    {
        public string firstName, givenName, gender;
        public int id;
    }

    [JsonObject]
    public class Skill
    {
        public int id;
        public string descriptionSpriteName;
    }

    public enum GachaType
    {
        normal = 0,
        beginner = 2,
        ceil = 1
    }

    [JsonObject]
    public class Gacha
    {
        public long startAt, endAt;
        public int id;
        public GachaBehaviour[] gachaBehaviors;
        public GachaType gachaType;
        public bool IsAvailable
        {
            get
            {
                var ts = DateTime.Now.GetTimestamp();
                return ts >= startAt && ts <= endAt && gachaType != GachaType.beginner;
            }
        }
    }

    [JsonObject]
    public class GachaBehaviour
    {
        public int id, gachaId, costResourceQuantity;

        public Gacha Gacha => MasterData.Instance.gachas.Single(ga => ga.id == gachaId);
    }

    [JsonObject]
    public class MasterData
    {
        public GameCharacter[] gameCharacters;
        public Card[] cards;
        public Skill[] skills;
        public Music[] musics;
        public Gacha[] gachas;
        public MusicDifficulty[] musicDifficulties;
        public MusicVocal[] musicVocals;
        public Event[] events;

        private static long Timestamp => (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000;

        [JsonIgnore]
        public GachaBehaviour[] gachaBehaviours;

        public static MasterData Instance { get; private set; }

        public Event CurrentEvent => events.LastOrDefault(e => e.startAt < Timestamp) ?? events.Last();

        public static async Task Initialize(SekaiClient client)
        {
            var fn = client.environment.X_Data_Version + ".json";

            try
            {
                Instance = JObject.Parse(File.ReadAllText(fn)).ToObject<MasterData>();
            }
            catch
            {
                Console.WriteLine("fetchinig master data...");
                var master = await client.CallApi("/suite/master", HttpMethod.Get, null);
                File.WriteAllText(fn, master.ToString(Formatting.Indented));
                Instance = master.ToObject<MasterData>();

            }

            Instance.gachaBehaviours = Instance.gachas.SelectMany(g => g.gachaBehaviors).ToArray();
        }
    }
}
