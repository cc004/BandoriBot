using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SekaiClient
{
    [JsonObject]
    public class User
    {
        public string uid, credit;
    }
}
