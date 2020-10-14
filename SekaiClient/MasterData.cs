using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SekaiClient.Datas
{
    [JsonObject]
    public class MusicInfo
    {

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
    }
    [JsonObject]
    public class Music
    {
        public MusicInfo music;
        public MusicDifficulty[] musicDifficulties;
        public MusicVocal[] musicVocals;
    }

    [JsonObject]
    public class Card
    {
        public int characterId, rarity, skillId;
        public string prefix, attr;
    }

    [JsonObject]
    public class Character
    {
        public string firstName, givenName, gender;
    }

    [JsonObject]
    public class Skill
    {
        public string descriptionSpriteName;
    }

    public static class MasterData
    {
        public static readonly Dictionary<string, Character> characters;
        public static readonly Dictionary<string, Card> cards;
        public static readonly Dictionary<string, Skill> skills;
        public static readonly Dictionary<string, Music> musics;

        static MasterData()
        {
            try
            {
                var master = JObject.Parse(File.ReadAllText("master_data.json"));
                characters = master["gameCharacters"].ToObject<Dictionary<string, Character>>();
                cards = master["cards"].ToObject<Dictionary<string, Card>>();
                skills = master["skills"].ToObject<Dictionary<string, Skill>>();
                musics = master["musicAlls"].ToObject<Dictionary<string, Music>>();
            }
            catch
            {

            }
        }
    }
}
