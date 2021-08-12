using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace BandoriBot.Services
{
    public static unsafe class RecordDatabaseManager
    {
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int RecordLength();
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FlushFile();
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void AddRecord(long qq, long group, long timestamp, [MarshalAs(UnmanagedType.LPWStr)] string message);
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CacheIndex(int start, int end);
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern bool RecordContains(IntPtr cache, int index, char* substr);
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void ReadRecord(IntPtr cache, int index, out long qq, out long group, out long timestamp, IntPtr* message);
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void FreeCache(IntPtr cache);
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void OpenFile(string datafile, string indexfile);
        [DllImport("database.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void CloseFile();
        public static int Length => RecordLength();
        private static readonly object @lock = new();
        public static void Close() => CloseFile();
        public static void InitDatabase()
        {
            OpenFile("record.dat", "index.dat");
        }

        // increase seplen will increase speed, at the cost of memory usage
        private const int seplen = 1000000;
        private const int cacheLength = 100;

        private static int n = 0;

        public static void AddRecord(long qq, long group, DateTime time, string message)
        {
            lock (@lock)
            {
                AddRecord(qq, group, time.ToTimestamp(), message);
                if (++n == cacheLength)
                {
                    n = 0;
                    FlushFile();
                }
            }
        }

        public static void ReadRecord(IntPtr cache, int index, out long qq, out long group, out long timestamp, out string message)
        {
            IntPtr pstr;
            ReadRecord(cache, index, out qq, out group, out timestamp, &pstr);
            message = Marshal.PtrToStringUni(pstr);
        }

        public static int CountContains(string substr)
        {
            if (string.IsNullOrEmpty(substr)) return RecordLength();
            IntPtr unmanaged = Marshal.StringToHGlobalUni(substr);
            char* pstr = (char*)unmanaged;
            // reduce marshal cost
            try
            {
                var result = 0;
                var n = RecordLength();
                for (int i = 0; i < n; i += seplen)
                {
                    var len = Math.Min(n - i, seplen);
                    var cache = CacheIndex(i, i + len);
                    try
                    {
                        result += Enumerable.Range(i, len).AsParallel().Count(i => RecordContains(cache, i, pstr));
                    }
                    finally
                    {
                        FreeCache(cache);
                    }
                }
                return result;
            }
            finally
            {
                Marshal.FreeHGlobal(unmanaged);
            }
        }


        public static IEnumerable<Record> GetRecords()
        {
            var n = RecordLength();
            for (int i = 0; i < n; i += seplen)
            {
                var len = Math.Min(n - i, seplen);
                var cache = CacheIndex(i, i + len);
                try
                {
                    for (int j = 0; j < len; ++j)
                    {
                        ReadRecord(cache, j + i, out var qq, out var group, out var ts, out string msg);
                        yield return new Record
                        {
                            group = group,
                            qq = qq,
                            message = msg,
                            timestamp = ts
                        };
                    }
                }
                finally
                {
                    FreeCache(cache);
                }
            }
        }
    }

    public class Record
    {
        public long qq { get; set; }
        public long group { get; set; }
        public long timestamp { get; set; }
        public string message { get; set; }
    }
}
