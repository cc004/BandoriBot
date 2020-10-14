using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SekaiClient
{
    public class ProxyPool
    {
        public HashSet<IPEndPoint> proxys = new HashSet<IPEndPoint>();

        public void GetProxysFromAPIs()
        {
            Utils.Log("Proxy", "正在使用API获取代理..");

            GetProxysFromAPI("http://www.66ip.cn/mo.php?tqsl=9999");
            GetProxysFromAPI("http://www.89ip.cn/tqdl.html?api=1&num=9999");

            Utils.Log("Proxy", "代理更新完成!数量:" + proxys.Count);
        }

        private void GetProxysFromAPI(string url)
        {
            string ips = Utils.SendGet(url);
            string pattern = "\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\:\\d{1,5}";

            foreach (Match match in Regex.Matches(ips, pattern))
            {
                var splits = match.Value.Split(':');
                IPEndPoint proxy = new IPEndPoint(IPAddress.Parse(splits[0]), int.Parse(splits[1]));
                proxys.Add(proxy);
            }
        }

    }
}
