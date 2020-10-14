using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MessagePack;

namespace SekaiClient
{
    public static class PackHelper
    {
        private static readonly byte[] key = Encoding.ASCII.GetBytes("g2fcC0ZczN9MTJ61");
        private static readonly byte[] iv = Encoding.ASCII.GetBytes("msx3IV0i9XE5uYZ1");
        public static JToken Unpack(byte[] crypted)
        {
            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decrypted = aes.CreateDecryptor().TransformFinalBlock(crypted, 0, crypted.Length);
            var obj = MessagePackSerializer.ConvertToJson(decrypted);
            return string.IsNullOrEmpty(obj) ? null : JToken.Parse(obj);
        }


        public static byte[] Pack(JToken content)
        {
            var aes = Aes.Create();
            aes.Key = key;
            aes.IV = iv;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var decrypted = content == null ? Array.Empty<byte>() : MessagePackSerializer.ConvertFromJson(content.ToString());
            return aes.CreateEncryptor().TransformFinalBlock(decrypted, 0, decrypted.Length);
        }
    }
}
