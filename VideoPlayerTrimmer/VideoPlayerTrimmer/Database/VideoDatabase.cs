using SQLite;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Database.Models;

namespace VideoPlayerTrimmer.Database
{
    public class VideoDatabase
    {
        private readonly SQLiteAsyncConnection connectionAsync;

        public VideoDatabase(string dbPath)
        {
            connectionAsync = new SQLiteAsyncConnection(dbPath);
        }

        public Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> condition) where T : new()
        {
            return connectionAsync.Table<T>().FirstOrDefaultAsync(condition);
        }

        public Task<List<T>> GetAsync<T>(Expression<Func<T,object>> order, Expression<Func<T,bool>> condition = null) where T: new()
        {
            if (condition == null)
            {
                return connectionAsync.Table<T>().OrderBy(order).ToListAsync();
            }
            return connectionAsync.Table<T>().Where(condition).OrderBy(order).ToListAsync();
        }

        public async Task InsertAsync<T>(T item)
        {
            await connectionAsync.InsertAsync(item);// is id saved in item?
        }

        public async Task InsertAllAsync<T>(IEnumerable<T> items)
        {
            //await connection.RunInTransactionAsync(tran =>
            //{
            //    foreach (var item in items)
            //    {
            //        tran.Insert(item);
            //    }
            //});
            await connectionAsync.InsertAllAsync(items);
        }

        public async Task UpdateAsync<T>(T item)
        {
            await connectionAsync.UpdateAsync(item);
        }

        public async Task UpdateAllAsync<T>(IEnumerable<T> items)
        {
            //await connection.RunInTransactionAsync(tran =>
            //{
            //    foreach (var item in items)
            //    {
            //        tran.Update(item);
            //    }
            //});
            await connectionAsync.UpdateAllAsync(items);
        }

        public async Task DeleteAsync<T>(T item)
        {
            await connectionAsync.DeleteAsync<T>(item);
        }

        public async Task DeleteAsync<T>(Expression<Func<T, bool>> condition) where T : new()
        {
            var items = await connectionAsync.Table<T>().Where(condition).ToListAsync();
            await connectionAsync.RunInTransactionAsync(tran =>
            {
                foreach (var item in items)
                {
                    tran.Delete(item);
                }
            });
        }

        public async Task UpdateIsNewAsync(int videoId, bool isNew)
        {
            App.DebugLog($"UPDATE {nameof(VideoFileTable)} SET {nameof(VideoFileTable.IsNew)} = {isNew} WHERE {nameof(VideoFileTable.VideoId)} = {videoId}");
            await connectionAsync.ExecuteAsync($"UPDATE {nameof(VideoFileTable)} SET {nameof(VideoFileTable.IsNew)} = ? WHERE {nameof(VideoFileTable.VideoId)} = ?", isNew, videoId);
        }
    }
}
