namespace VideoPlayerTrimmer
{
    class PageNames
    {
        public const string Main = "main";

        public const string FoldersTab = "tabfolders";
        public const string FavoritesTab = "tabfav";
        public const string TrimmerTab = "tabtrim";
        public const string SettingsTab = "tabsett";

        public const string Folders = "folders";
        public const string Favourites = "favorites";
        public const string Trimmer = "trimmer";
        public const string Settings = "settings";

        public const string Videos = "videos";
        public const string Player = "player";

        public static readonly string FoldersNav = $"//{Main}/{FoldersTab}/{Folders}";
        public static readonly string FavoritesNav = $"//{Main}/{FavoritesTab}/{Favourites}";
        public static readonly string TrimmerNav = $"//{Main}/{TrimmerTab}/{Trimmer}";
        public static readonly string SettingsNav = $"//{Main}/{SettingsTab}/{Settings}";
    }
}
