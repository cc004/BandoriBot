﻿using MessagePack.ImmutableCollection;
using Newtonsoft.Json.Linq;
using SekaiClient.Datas;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace SekaiClient
{
    public class SekaiClient
    {
        public Action<string> DebugWrite = text => { };

        private const string urlroot = "http://production-game-api.sekai.colorfulpalette.org/api";
        private const string urlroot2 = "https://production-game-api.sekai.colorfulpalette.org/api";

        private bool connected = false;
        private readonly HttpClient client;
        private string adid, uid, token;
        internal readonly EnvironmentInfo environment;
        
        private static readonly BlockingCollection<SekaiClient> _staticClients = new ();
        public static SekaiClient GetClient()
        {
            if (!_staticClients.TryTake(out var _staticClient))
            {
                _staticClient = new SekaiClient(new EnvironmentInfo())
                {
                    DebugWrite = Console.WriteLine
                };
                _staticClient.Reset().Wait();

            }

            return _staticClient;
        }

        public static void PutClient(SekaiClient client)
        {
            _staticClients.Add(client);
        }

        public string AssetHash { get; private set; }

        private void SetupHeaders()
        {
            client.DefaultRequestHeaders.Clear();
            foreach (var field in typeof(EnvironmentInfo).GetFields())
            {
                if (field.FieldType != typeof(string)) continue;
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    field.Name.Replace('_', '-'),
                    field.GetValue(environment) as string);
            }
        }

        public async Task Reset()
        {
            InitializeAdid();
            await UpgradeEnvironment();
            var user = await Register();
            await Login(user);
            await MasterData.Initialize(this);
            await PassTutorial(true);
        }

        private CookieContainer cookie = new ();
        
        public SekaiClient(EnvironmentInfo info, bool useProxy = false)
        {
            var headertype = typeof(HttpClient).Assembly.GetType("System.Net.Http.Headers.HttpHeaderType");
            environment = info;

            if (useProxy)
            {
                throw new Exception("No proxy");
            }
            else
            {
                client = new HttpClient(new HttpClientHandler
                {
                    CookieContainer = cookie
                });
                client.Timeout = new TimeSpan(0, 10, 0);
                typeof(HttpHeaders).GetField("_allowedHeaderTypes", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(client.DefaultRequestHeaders, Enum.Parse(headertype, "All"));
                SetupHeaders();
            }

                //client.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");

            }

        public async Task<JToken> CallApi(string apiurl, HttpMethod method, JObject content)
        {
            if (token != null)
                client.DefaultRequestHeaders.TryAddWithoutValidation("X-Session-Token", token);
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-Request-Id", Guid.NewGuid().ToString());

            try
            {

                var tick = DateTime.Now.Ticks;


                HttpResponseMessage resp;

                try
                {
                    resp = await client.SendAsync(
                        new HttpRequestMessage(method, (connected ? urlroot2 : urlroot) + apiurl)
                        {
                            Content = method == HttpMethod.Get ? null : new ByteArrayContent(PackHelper.Pack(content))
                        });
                }
                catch (AggregateException)
                {
                    throw new ApiException($"与服务器通信时发生错误, 请求超时");
                }

                if (!resp.IsSuccessStatusCode)
                {
                    if (resp.StatusCode == HttpStatusCode.UpgradeRequired)
                    {
                        DebugWrite(
                            $"api failed with header = {string.Join("\n", client.DefaultRequestHeaders.Select(pair => $"{pair.Key}={string.Join(",", pair.Value)}"))}");
                    }

                    if (resp.StatusCode == HttpStatusCode.Forbidden)
                    {
                        DebugWrite("Cookie expired, refreshing...");

                        client.DefaultRequestHeaders.Remove("Cookie");
                        resp = await client.PostAsync(
                            "https://issue.sekai.colorfulpalette.org/api/signature",
                            new ByteArrayContent(PackHelper.Pack(null))
                            );
                        foreach (var cookie in resp.Headers.GetValues("Set-Cookie").First().Split(';'))
                            this.cookie.SetCookies(new Uri(urlroot), cookie);
                        return await CallApi(apiurl, method, content);
                    }
                    throw new ApiException($"与服务器通信时发生错误, HTTP {(int) resp.StatusCode} {resp.StatusCode}");
                }

                var nextToken = resp.Headers.Contains("X-Session-Token")
                    ? resp.Headers.GetValues("X-Session-Token").Single()
                    : null;
                if (nextToken != null) token = nextToken;
                var result = PackHelper.Unpack(await resp.Content.ReadAsByteArrayAsync());

                connected = true;

                DebugWrite(apiurl + $" called, {(DateTime.Now.Ticks - tick) / 1000 / 10.0} ms elapsed");
                return result;
            }
            finally
            {
                client.DefaultRequestHeaders.Remove("X-Session-Token");
                client.DefaultRequestHeaders.Remove("X-Request-Id");
            }
        }

        public async Task<JToken> CallUserApi(string apiurl, HttpMethod method, JObject content)
            => await CallApi($"/user/{uid}" + apiurl, method, content);

        public void InitializeAdid()
        {

            using var client = new HttpClient();
            var json = new JObject
            {
                ["initiated_by"] = "sdk",
                ["apilevel"] = "29",
                ["event_buffering_enabled"] = "0",
                ["app_version"] = environment.X_App_Version,
                ["app_token"] = "6afszmodmiv4",
                ["os_version"] = "10",
                ["device_type"] = "phone",
                ["gps_adid"] = "20a417f1-46cb-4b26-9749-9b709be8ba60",
                ["android_uuid"] = "55d9b15f-3dbf-4e9e-b270-67351555b6db",
                ["device_name"] = "SEA-AL10",
                ["environment"] = "production",
                ["needs_response_details"] = "1",
                ["attribution_deeplink"] = "1",
                ["package_name"] = "com.sega.pjsekai",
                ["os_name"] = "android",
                ["gps_adid_src"] = "service",
                ["tracking_enabled"] = "1",
            };

            //json = JObject.Parse(client.PostAsync("https://app.adjust.com/session", new StringContent(json.ToString())).Result.Content.ReadAsStringAsync().Result);

            adid = "20f48346fad7f921245a8db7fdfb734f";
            client.DefaultRequestHeaders.TryAddWithoutValidation("X-AI", adid);
        }

        public async Task Login(User user)
        {
            uid = user.uid;
            var json = await CallUserApi($"/auth?refreshUpdatedResources=False", HttpMethod.Put, new JObject
            {
                ["credential"] = user.credit
            });
            token = json["sessionToken"].ToString();
            AssetHash = json.Value<string>("assetHash");

            DebugWrite($"authenticated as {user.uid}");
        }

        public async Task<User> Register()
        {
            var json = await CallApi("/user", HttpMethod.Post, environment.CreateRegister());
            var uid = json["userRegistration"]["userId"].ToString();
            var credit = json["credential"].ToString();
            DebugWrite($"registered user {uid}");

            return new User
            {
                uid = uid,
                credit = credit
            };
        }

        public async Task<int> PassTutorial(bool simplified = false)
        {
            //bypass turtorials
            await CallUserApi($"/tutorial", HttpMethod.Patch, new JObject { ["tutorialStatus"] = "opening_1" });
            await CallUserApi($"/tutorial", HttpMethod.Patch, new JObject { ["tutorialStatus"] = "gameplay" });
            await CallUserApi($"/tutorial", HttpMethod.Patch, new JObject { ["tutorialStatus"] = "opening_2" });
            await CallUserApi($"/tutorial", HttpMethod.Patch, new JObject { ["tutorialStatus"] = "unit_select" });
            await CallUserApi($"/tutorial", HttpMethod.Patch, new JObject { ["tutorialStatus"] = "idol_opening" });
            await CallUserApi($"/tutorial", HttpMethod.Patch, new JObject { ["tutorialStatus"] = "summary" });
            var presents = (await CallUserApi($"/home/refresh", HttpMethod.Put, new JObject { ["refreshableTypes"] = new JArray("login_bonus") }))["updatedResources"]["userPresents"]
                .Select(t => t.Value<string>("presentId")).ToArray();
            await CallUserApi($"/tutorial", HttpMethod.Patch, new JObject { ["tutorialStatus"] = "end" });
            if (simplified) return 0;
            var episodes = new int[] { 50000, 50001, 40000, 40001, 30000, 30001, 20000, 20001, 60000, 60001, 4, 8, 12, 16, 20 };
            foreach (var episode in episodes)
                await CallUserApi($"/story/unit_story/episode/{episode}", HttpMethod.Post, null);
            await CallUserApi($"/present", HttpMethod.Post, new JObject { ["presentIds"] = new JArray(presents) });
            await CallUserApi($"/costume-3d-shop/20006", HttpMethod.Post, null);
            await CallUserApi($"/shop/2/item/10012", HttpMethod.Post, null);
            var currency = (await CallUserApi($"/mission/beginner_mission", HttpMethod.Put, new JObject
            {
                ["missionIds"] = new JArray(1, 2, 3, 4, 5, 6, 8, 10)
            }))["updatedResources"]["user"]["userGamedata"]["chargedCurrency"].Value<int>("free");

            DebugWrite($"present received, now currency = {currency}");
            return currency;
        }

        public async Task<IEnumerable<Card>> Gacha(GachaBehaviour behaviour)
        {
            return (await CallUserApi($"/gacha/{behaviour.gachaId}/gachaBehaviorId/{behaviour.id}", HttpMethod.Put, null))["obtainPrizes"]
                .Select(t => MasterData.Instance.cards.Single(c => c.id == t["card"].Value<int>("resourceId")));
        }

        public async Task<string[]> Gacha(int currency)
        {
            IEnumerable<Card> icards = new Card[0];

            var rolls = MasterData.Instance.gachaBehaviours
                .GroupBy(b => (b.costResourceQuantity, b.costResourceType))
                .Select(g => g.Where(b => b.Gacha.IsAvailable)
                    .OrderByDescending(b => b.Gacha.rarity4Rate)
                    .ThenByDescending(b => b.Gacha.endAt).FirstOrDefault())
                .Where(g => g != null).ToArray();

            var roll10 = rolls.Single(b => b.costResourceQuantity == 3000 && b.costResourceType == CostResourceType.jewel);
            var roll1 = rolls.Single(b => b.costResourceQuantity == 300 && b.costResourceType == CostResourceType.jewel);
            var roll3 = rolls.Single(b => b.costResourceQuantity == 1 && b.costResourceType == CostResourceType.gacha_ticket);

            //icards = await Gacha(roll3);
            
            icards = Array.Empty<Card>();

            while (currency > 3000)
            {
                currency -= 3000;
                icards = icards.Concat(await Gacha(roll10));
            }
            /*
            while (currency > 300)
            {
                currency -= 300;
                icards = icards.Concat(await Gacha(roll1));
            }*/
            var cards = icards.ToArray();
            var desc = cards
                .Select(card =>
                {
                    var character = MasterData.Instance.gameCharacters.Single(gc => gc.id == card.characterId);
                    var skill = MasterData.Instance.skills.Single(s => s.id == card.skillId);
                    return $"[{card.prefix}]{character.firstName}{character.givenName} " + new string(Enumerable.Range(0, card.rarity).Select(_ => '*').ToArray());
                }).ToArray();

            DebugWrite($"gacha result:\n" + string.Join('\n', desc));
            int[] rares = new int[5];
            foreach (var card in cards) ++rares[card.rarity];
            if (rares[4] > 1)
                Console.WriteLine($"gacha result: {rares[4]}, {rares[3]}, {rares[2]}");
            return desc;
        }

        public async Task<string> Inherit(string password)
        {
            return (await CallUserApi("/inherit", HttpMethod.Put, new JObject { ["password"] = password }))["userInherit"].Value<string>("inheritId");
        }

        public async Task UpgradeEnvironment()
        {
            var data = await CallApi("/system", HttpMethod.Get, null);
            var myver = data["appVersions"].FirstOrDefault(t => t.Value<string>("systemProfile") == "production" && t.Value<string>("appVersionStatus") == "available");
            environment.X_App_Version = myver.Value<string>("appVersion");
            environment.X_Asset_Version = myver.Value<string>("assetVersion");
            environment.X_Data_Version = myver.Value<string>("dataVersion");
            DebugWrite($"using ver={myver}");
            SetupHeaders();
        }

        public async Task<Account> Serialize(string[] cards, string password = "") => new Account
        {
            inheritId = await Inherit(password),
            password = password,
            cards = cards
        };

        public async Task<int> APLive(int musicId, int boostCount, int deckId, string musicDifficulty = "expert", int score=100000000)
        {
            var music = MasterData.Instance.musics.Single(m => m.id == musicId);
            var md = MasterData.Instance.musicDifficulties.Single(md => md.musicDifficulty == musicDifficulty);
            var result = (await CallUserApi("/live", HttpMethod.Post, new JObject
            {
                ["musicId"] = musicId,
                ["musicDifficultyId"] = md.id,
                ["musicVocalId"] = MasterData.Instance.musicVocals.First(mv => mv.musicId == musicId).id,
                ["deckId"] = deckId,
                ["boostCount"] = boostCount
            }));

            var liveid = result["userLiveId"];

            result = await CallUserApi("/live/" + liveid, HttpMethod.Put, new JObject
            {
                ["score"] = score,
                ["perfectCount"] = md.noteCount,
                ["greatCount"] = 0,
                ["goodCount"] = 0,
                ["badCount"] = 0,
                ["missCount"] = 0,
                ["maxCombo"] = md.noteCount,
                ["life"] = 1000,
                ["tapCount"] = md.noteCount,
                ["continueCount"] = 0
            });

            DebugWrite($"ap live done, now pt = {result["afterEventPoint"]}, currency = {result["updatedResources"]["user"]["userGamedata"]["chargedCurrency"]["free"]}" );
            return (int)result["afterEventPoint"];
        }

        public async Task<int[]> GetCards()
        {
            var data = await CallApi($"/suite/user/{uid}", HttpMethod.Get, null);

            return data["userCards"].Select(t => t.Value<int>("cardId")).ToArray();
        }

        public async Task ChangeDeck(int deckId, int[] cardIds)
        {
            await CallUserApi("/deck/" + deckId, HttpMethod.Put, new JObject
            {
                ["userId"] = uid,
                ["deckId"] = deckId,
                ["name"] = "deck" + deckId,
                ["leader"] = cardIds[0],
                ["member1"] = cardIds[0],
                ["member2"] = cardIds[1],
                ["member3"] = cardIds[2],
                ["member4"] = cardIds[3],
                ["member5"] = cardIds[4],
            });
        }

    }
}
