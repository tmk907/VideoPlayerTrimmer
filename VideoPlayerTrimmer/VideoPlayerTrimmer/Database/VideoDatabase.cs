using SQLite;
using System;
using System.Collections.Generic;
using System.Text;
using VideoPlayerTrimmer.Database.Models;

namespace VideoPlayerTrimmer.Database
{
    public class VideoDatabase
    {
        private readonly SQLiteAsyncConnection database;

        public VideoDatabase(string dbPath)
        {
            database = new SQLiteAsyncConnection(dbPath);
        }

        public void CreateTables(string dbPath)
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
