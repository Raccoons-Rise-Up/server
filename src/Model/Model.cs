using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GameServer
{
    public class DatabaseContext : DbContext
    {
        public DbSet<Player> Players { get; set; }

        public string DbPath { get; private set; }

        public DatabaseContext()
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);
            DbPath = $"{path}{System.IO.Path.DirectorySeparatorChar}Database.db";
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
