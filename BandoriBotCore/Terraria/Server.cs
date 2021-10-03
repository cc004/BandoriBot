
using System;
using System.Linq;
using System.Net.Http;
using System.Web;
using BandoriBot.Commands;
using Newtonsoft.Json.Linq;

namespace BandoriBot.Terraria
{
    public class Server
    {
        private string token, endpoint;
        public int group;
        public bool display;
        public string noRegister;

        private static JToken GetHttp(string uri)
        {
            using var client = new HttpClient();
            client.Timeout = new TimeSpan(0, 0, 10);
            return JToken.Parse(client.GetAsync(uri).Result.Content.ReadAsStringAsync().Result);
        }

        public Server(string endpoint, int group)
        {
            this.endpoint = endpoint;
            this.group = group;
        }

        public Action Relogin;

        public void Login(string username, string password)
        {
            var tokenResult = GetHttp($"http://{endpoint}/v2/token/create?username={HttpUtility.UrlEncode(username)}&password={HttpUtility.UrlEncode(password)}");
            if (tokenResult["status"].ToString() != "200")
            {
                throw new CommandException("Fail to get token");
            }
            token = tokenResult["token"].ToString();
        }

        public JToken RunRest(string uri)
        {
            JToken result = null;
            bool retry = true;
            try
            {
                retry:
                result = GetHttp($"http://{endpoint}{uri}{(uri.Contains('?') ? "&" : "?")}token={token}");
                if (result is JObject obj)
                    if (obj.ContainsKey("status") && obj["status"].ToString() != "200" &&
                        obj["status"].ToString() != "success")
                    {
                        if (!retry) throw new Exception();
                        Relogin();
                        retry = false;
                        goto retry;
                    }
                return result;
            }
            catch
            {
                throw new CommandException("Rest调用失败，返回\n" + (result?.ToString() ?? "null"));
            }
        }

        public JToken RunCommand(string command)
        {
            return RunRest($"/v3/server/rawcmd?cmd={HttpUtility.UrlEncode(command)}");
        }


        public int GetItemStack(string name, int id)
        {
            return int.Parse(RunRest("/v1/itemrank/rankboard?&id=" + id)
                .Where(t => t["name"].ToString() == name).FirstOrDefault()["count"].ToString());
        }
        public string GetMoney(string name)
        {
            int copper = GetItemStack(name, 71),
                silver = GetItemStack(name, 72),
                gold = GetItemStack(name, 73),
                platinum = GetItemStack(name, 74);
            string res = "";
            if (platinum != 0)
            {
                res += platinum + "铂金";
            }
            if (gold != 0)
            {
                res += gold + "金";
            }
            if (silver != 0)
            {
                res += silver + "银";
            }
            if (copper != 0)
            {
                res += copper + "铜";
            }
            if (res == "") res = "无产阶级";
            return res;
        }
        public string[] GetOnlinePlayers()
        {
            try
            {
                return RunRest("/v2/users/activelist")["activeusers"]
                    .ToString().Split('\t').Where((s) => !string.IsNullOrWhiteSpace(s)).ToArray();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }
    }
}
