using System;
using System.IO;
using System.Collections.Generic;
using GameServer.Server;
using Microsoft.EntityFrameworkCore;

namespace GameServer.Database
{
    public class DatabaseContext : DbContext
    {
        public DbSet<ModelPlayer> Players { get; set; }

        public string DbPath { get; private set; }

        public DatabaseContext()
        {
            // NOTE: ENetServer.AppDataPath can NOT be used here as this code is not executed when DatabaseContext() is created!

            var folder = Environment.SpecialFolder.LocalApplicationData;
            var path = Environment.GetFolderPath(folder);

            // Windows: C:\Users\USER\AppData\Local\ENet Server\Database.db (Delete this file if it is corrupt)
            DbPath = $"{path}{Path.DirectorySeparatorChar}ENet Server{Path.DirectorySeparatorChar}Database.db";
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite($"Data Source={DbPath}");
    }
}
