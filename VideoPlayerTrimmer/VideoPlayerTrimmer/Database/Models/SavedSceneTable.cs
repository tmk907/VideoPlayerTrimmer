using SQLite;

namespace VideoPlayerTrimmer.Database.Models
{
    public class SavedSceneTable
    {
        [PrimaryKey, AutoIncrement]
        public int SavedSceneId { get; set; }
        public int FavSceneId { get; set; }
        public int MediaStoreId { get; set; }
    }
}
