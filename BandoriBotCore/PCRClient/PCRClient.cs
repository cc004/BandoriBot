using BandoriBot.Handler;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCRClient
{
    public class PCRClient
    {
        private readonly HttpClient client;
        private const string urlroot = "https://l3-prod-all-gs-gzlj.bilibiligame.net/";
        private long viewer_id;
        private string request_id;
        private string session_id;
        private readonly EnvironmentInfo environment;
        public JObject Load { get; private set; }
        public JObject Home { get; private set; }

        public int ClanId => Home["user_clan"].Value<int>("clan_id");
        
        private int _clanbattleid = 0;
        public int ClanBattleid
        {
            get
            {
                if (_clanbattleid == 0)
                {
                    _clanbattleid = Callapi("clan_battle/top", new JObject
                    {
                        ["clan_id"] = ClanId,
                        ["is_first"] = 1,
                        ["current_clan_battle_coin"] = 0
                    }).Value<int>("clan_battle_id");
                }

                return _clanbattleid;
            }
        }

        public PCRClient(EnvironmentInfo info)
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            foreach (var field in typeof(EnvironmentInfo).GetFields())
            {
                if (field.FieldType != typeof(string)) continue;
                client.DefaultRequestHeaders.TryAddWithoutValidation(
                    field.IsDefined(typeof(NoUpperAttribute), true) ?
                        field.Name.Replace('_', '-') : 
                        field.Name.Replace('_', '-').ToUpper(),
                    field.GetValue(info) as string);
            }

            client.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "Keep-Alive");
            client.Timeout = new TimeSpan(0, 0, 10);
            environment = info;
            viewer_id = info.viewer_id;
        }

        public JToken Callapi(string apiurl, JObject request, bool crypted = true)
        {
            var key = PackHelper.CreateKey();
            request.Add("viewer_id", crypted ? PackHelper.Encrypt(viewer_id.ToString(), key) : viewer_id.ToString());
            var req = PackHelper.Pack(request, key);

            bool flag = request_id != null, flag2 = session_id != null;
            if (flag) client.DefaultRequestHeaders.TryAddWithoutValidation("REQUEST-ID", request_id);
            if (flag2) client.DefaultRequestHeaders.TryAddWithoutValidation("SID", session_id);
            var resp = client.PostAsync(urlroot + apiurl, crypted ? new ByteArrayContent(req) : new StringContent(request.ToString())).Result;
            if (flag) client.DefaultRequestHeaders.Remove("REQUEST-ID");
            if (flag2) client.DefaultRequestHeaders.Remove("SID");

            var respdata = resp.Content.ReadAsStringAsync().Result;
            var json = crypted ? PackHelper.Unpack(Convert.FromBase64String(respdata), out var _) : JObject.Parse(respdata);

            var header = json["data_headers"] as JObject;
            if (header.TryGetValue("sid", out var sid) && !string.IsNullOrEmpty((string)sid))
            {
                using var md5 = MD5.Create();
                session_id = string.Concat(md5.ComputeHash(Encoding.UTF8.GetBytes((string)sid + "c!SID!n")).Select(b => b.ToString("x2")));
            }

            if (header.TryGetValue("request_id", out var rid) && (string)rid != request_id)
            {
                request_id = (string)rid;
            }

            if (header.TryGetValue("viewer_id", out var vid) && (long?)vid != null && (long)vid != viewer_id)
            {
                viewer_id = (long)vid;
            }

            if (json["data"] is JObject obj)
                if (obj.TryGetValue("server_error", out var obj2))
                    throw new ApiException($"{obj2["title"]}: {obj2["message"]} (code = {obj2["status"]})");

            return json["data"] as JToken;
        }

        public void Login(string uid, string access_key)
        {
            var manifest = Callapi("source_ini/get_maintenance_status?format=json", new JObject(), false);
            var ver = (string)manifest["required_manifest_ver"];

            Console.WriteLine($"using manifest: " + ver);
            client.DefaultRequestHeaders.TryAddWithoutValidation("MANIFEST-VER", ver);

            Callapi("tool/sdk_login", new JObject
            {
                ["uid"] = uid,
                ["access_key"] = access_key,
                ["channel"] = environment.channel_id,
                ["platform"] = environment.platform_id
            });

            Callapi("check/game_start", new JObject
            {
                ["app_type"] = 0,
                ["campaign_data"] = "",
                ["campaign_user"] = new Random().Next(0, 100000),
            });

            Callapi("check/check_agreement", new JObject());

    
            Load = Callapi("load/index", new JObject { ["carrier"] = "OPPO" }) as JObject;
            Home = Callapi("home/index", new JObject
            {
                ["message_id"] = 1,
                ["tips_id_list"] = new JArray(),
                ["is_first"] = 1,
                ["gold_history"] = 0
            }) as JObject;
        }
    }
}
