using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using VideoPlayerTrimmer.Database.Models;
using Xamarin.Essentials;

namespace VideoPlayerTrimmer.Database
{
    public class DatabaseSetup
    {
        private string dbPath;

        public DatabaseSetup(string dbPath)
        {
            this.dbPath = dbPath;
        }

        public void PerformMigrations()
        {
            if (VersionTracking.IsFirstLaunchEver)
            {
                CreateTables();
            }
        }

        private void CreateTables()
        {
            using (var connection = new SQLiteConnection(dbPath))
            {
                connection.CreateTable<VideoFileTable>();
                connection.CreateTable<SavedSceneTable>();
                connection.CreateTable<FavoriteSceneTable>();
            }
        }
    }
}
