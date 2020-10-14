using System;
using System.IO;
using System.Linq;

namespace SekaiClient
{
    public static class Extensions
    {
        public static byte[] ReadToEnd(this Stream stream)
        {
            const int bufferSize = 4096;
            using var ms = new MemoryStream();
            byte[] buffer = new byte[bufferSize];
            int count;
            while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                ms.Write(buffer, 0, count);
            return ms.ToArray();
        }

        public static string PadRightEx(this string str, int length)
        {
            return str + new string(Enumerable.Range(0, Math.Max(0, length - str.Sum(c => c > 127 ? 2 : 1))).Select(_ => ' ').ToArray());
        }
    }
}
