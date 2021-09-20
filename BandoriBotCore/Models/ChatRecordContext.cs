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
        public static ChatRecordContext Context = new();

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
}
