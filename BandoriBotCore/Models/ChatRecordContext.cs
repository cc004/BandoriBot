using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BandoriBot.Models
{
    public class Record
    {
        public int Id { get; set; }
        public long QQ { get; set; }
        public long Group { get; set; }
        public string Message { get; set; }
        public DateTime Time { get; set; }
    }

    public class ChatRecordContext : DbContext
    {
        [ThreadStatic]
        public static ChatRecordContext _context;
        public static ChatRecordContext Context => _context ??= new();
        public static ChatRecordContext context = new();

        public virtual DbSet<Record> Records { get; set; }

        public ChatRecordContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("data source=records.db");
        }
    }
    public class KeywordRecord
    {
        public string Keyword { get; set; }
        public int Count { get; set; }
    }

    public class KeywordRecordContext : DbContext
    {
        public static KeywordRecordContext context = new();

        public virtual DbSet<KeywordRecord> Records { get; set; }

        public KeywordRecordContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<KeywordRecord>().HasKey(x => x.Keyword);
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("data source=keywordrecord.db");
        }
    }
}
