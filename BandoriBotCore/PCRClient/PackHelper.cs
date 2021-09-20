using MsgPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PCRClient
{
    public static class PackHelper
    {
        public static JToken Unpack(byte[] crypted, out byte[] key)
        {

            RijndaelManaged aes = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128,
                Padding = PaddingMode.PKCS7
            };
            var n = crypted.Length;
            key = crypted.Skip(n - 32).ToArray();
            var transform = aes.CreateDecryptor(key, Encoding.UTF8.GetBytes("ha4nBYA2APUD6Uv1"));
            var buf = transform.TransformFinalBlock(crypted.Take(n - 32).ToArray(), 0, n - 32);
            return JToken.FromObject(new BoxingPacker().Unpack(buf));
        }


        public static string Encrypt(string content, byte[] key)
        {

            RijndaelManaged aes = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128,
                Padding = PaddingMode.PKCS7
            };

            var transform = aes.CreateEncryptor(key, Encoding.UTF8.GetBytes("ha4nBYA2APUD6Uv1"));

            var buf = Encoding.UTF8.GetBytes(content);

            return Convert.ToBase64String(transform.TransformFinalBlock(buf, 0, buf.Length).Concat(key).ToArray());
        }

        public static byte[] CreateKey() => Encoding.ASCII.GetBytes(string.Concat(Guid.NewGuid().ToByteArray().Select(b => b.ToString("x2"))));

        public static byte[] Pack(JToken token, byte[] key)
        {

            RijndaelManaged aes = new RijndaelManaged
            {
                Mode = CipherMode.CBC,
                KeySize = 256,
                BlockSize = 128,
                Padding = PaddingMode.PKCS7
            };

            var transform = aes.CreateEncryptor(key, Encoding.UTF8.GetBytes("ha4nBYA2APUD6Uv1"));

            Func<JToken, object> mapper = null;

            mapper = o =>
            {
                if (o is JArray arr) return arr.Select(mapper).ToArray();
                else if (o is JObject obj)
                {
                    var dict = new Dictionary<string, object>();
                    foreach (var prop in obj.Properties())
                        dict.Add(prop.Name, mapper(prop.Value));
                    return dict;
                }
                else return o.ToObject<object>();
            };

            var mapped = mapper(token);

            var buf = new BoxingPacker().Pack(mapped);

            return transform.TransformFinalBlock(buf, 0, buf.Length).Concat(key).ToArray();
        }
    }
}
