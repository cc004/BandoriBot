using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using BandoriBot.Models;
using Microsoft.EntityFrameworkCore;

namespace BandoriBot.Services
{
    public class RecordContext : DbContext
    {
        [ThreadStatic]
        public static RecordContext Instance;

        public DbSet<Record> records { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=records.db");
        }
    }
    public class Record
    {
        public long id { get; set; }
        public long qq { get; set; }
        public long group { get; set; }
        public long timestamp { get; set; }
        public string message { get; set; }
    }
    public static unsafe class RecordDatabaseManager
    {
        public static int Length => RecordContext.Instance.records.Count();

        public static void Close()
        {
        }

        public static void InitDatabase()
        {
            if (RecordContext.Instance == null)
            {
                RecordContext.Instance = new();
            }

            RecordContext.Instance.Database.EnsureCreated();
            Utils.Log(LoggerLevel.Info, $"record count = {RecordContext.Instance.records.Count()}");
        }

        private static DateTime last;
        public static void AddRecord(long qq, long group, DateTime time, string message)
        {
            if (RecordContext.Instance == null)
            {
                RecordContext.Instance = new();
            }
            var now = DateTime.Now;
            RecordContext.Instance.records.Add(new Record()
            {
                group = group,
                message = message,
                qq = qq,
                timestamp = time.ToTimestamp()
            });
            if (last + TimeSpan.FromMilliseconds(10) < now)
            {
                last = now;
                RecordContext.Instance.SaveChanges();
            }
        }
        
        public static int CountContains(string substr)
        {
            return RecordContext.Instance.records.Count(r => r.message.Contains(substr));
        }


        public static DbSet<Record> GetRecords()
        {
            return RecordContext.Instance.records;
        }
    }

}
