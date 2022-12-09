using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SekaiClient
{
    public class EnvironmentInfo
    {
        public string Content_Type = "application/octet-stream";
        public string Accept = "application/octet-stream";
        public string Accept_Encoding = "deflate, gzip";
        // public string Host = "production-game-api.sekai.colorfulpalette.org";
        public string User_Agent = "UnityPlayer/2019.4.3f1 (UnityWebRequest/1.0, libcurl/7.52.0-DEV)";
        public string X_Install_Id = Guid.NewGuid().ToString();//"3ef222d3-6de3-43bf-aa19-9830b9bffc08";
        public string X_App_Version = "1.0.1";
        public string X_Asset_Version = "1.0.10";
        public string X_Data_Version = "1.0.1.3";
        public string X_Platform = "Android";
        public string X_DeviceModel = "HUAWEI SEA-AL10";
        public string X_OperatingSystem = "Android OS 10 / API-29 (HUAWEISEA-AL10/10.1.0.164C00)";
        public string X_MA = "F8:9A:78:5F:47:2D";
        public string X_Unity_Version = "2019.4.3f1";

        internal JObject CreateRegister()
        {
            return new JObject
            {
                ["platform"] = X_Platform,
                ["deviceModel"] = X_DeviceModel,
                ["operatingSystem"] = X_OperatingSystem
            };
        }
    }
}
