
using System;
using System.Net.Http;
using System.Web;
using BandoriBot.Commands;
using Newtonsoft.Json.Linq;

namespace Native.Csharp.App.Terraria
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
            try
            {
                result = GetHttp($"http://{endpoint}{uri}{(uri.Contains('?') ? "&" : "?")}token={token}");
                if (result is JObject obj)
                    if (obj.ContainsKey("status") && obj["status"].ToString() != "200" && obj["status"].ToString() != "success")
                        throw new Exception();
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
    }
}
