using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SekaiClient
{
    [JsonObject]
    public class Account
    {
        public string password;
        public string inheritId;
        public int nums;
        public string[] cards;
    }
}
