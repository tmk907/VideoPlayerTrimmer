using SQLite;

namespace VideoPlayerTrimmer.Database.Models
{
    public class SavedSceneTable
    {
        [PrimaryKey, AutoIncrement]
        public int SavedSceneId { get; set; }
        [NotNull]
        public int VideoId { get; set; }
        public int FavSceneId { get; set; }
    }
}
