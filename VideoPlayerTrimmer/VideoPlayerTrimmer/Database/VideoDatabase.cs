using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using VideoPlayerTrimmer.Database.Models;

namespace VideoPlayerTrimmer.Database
{
    public class VideoDatabase
    {
        private readonly SQLiteAsyncConnection connection;

        public VideoDatabase(string dbPath)
        {
            connection = new SQLiteAsyncConnection(dbPath);
        }

        public async Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> condition) where T : new()
        {
            return await connection.Table<T>().FirstOrDefaultAsync(condition);
        }

        public async Task<List<T>> GetAsync<T>(Expression<Func<T,object>> order, Expression<Func<T,bool>> condition = null) where T: new()
        {
            if (condition == null)
            {
                return await connection.Table<T>().OrderBy(order).ToListAsync();
            }
            return await connection.Table<T>().Where(condition).OrderBy(order).ToListAsync();
        }

        public async Task InsertAsync<T>(T item)
        {
            await connection.InsertAsync(item);// is id saved in item?
        }

        public async Task InsertAllAsync<T>(IEnumerable<T> items)
        {
            await connection.RunInTransactionAsync(tran =>
            {
                foreach (var item in items)
                {
                    tran.Insert(item);
                }
            });
        }

        public async Task UpdateAsync<T>(T item)
        {
            await connection.UpdateAsync(item);
        }

        public async Task UpdateAllAsync<T>(IEnumerable<T> items)
        {
            await connection.RunInTransactionAsync(tran =>
            {
                foreach (var item in items)
                {
                    tran.Update(item);
                }
            });
        }

        public async Task DeleteAsync<T>(T item)
        {
            await connection.DeleteAsync<T>(item);
        }

        public async Task DeleteAsync<T>(Expression<Func<T, bool>> condition) where T : new()
        {
            var items = await connection.Table<T>().Where(condition).ToListAsync();
            await connection.RunInTransactionAsync(tran =>
            {
                foreach (var item in items)
                {
                    tran.Delete(item);
                }
            });
        }

        public async Task UpdateIsNewAsync(int videoId, bool isNew)
        {
            App.DebugLog($"UPDATE {nameof(VideoFileTable)} SET {nameof(VideoFileTable.IsNew)} = ? WHERE {nameof(VideoFileTable.VideoId)} = ?");
            await connection.ExecuteAsync($"UPDATE {nameof(VideoFileTable)} SET {nameof(VideoFileTable.IsNew)} = ? WHERE {nameof(VideoFileTable.VideoId)} = ?", isNew, videoId);
        }
    }
}
