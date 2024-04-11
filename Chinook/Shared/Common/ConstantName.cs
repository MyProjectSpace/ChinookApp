namespace Chinook.Shared.Common
{
    public static class ConstantName
    {
        public const string FAVORITE_PLAYLIST_NAME = "Favorites";
        public const string FAVORITE_PLAYLIST_DISPLAY_NAME = "My favorite tracks";
        public const string ADD_COMMAND = "add";
        public const string REMOVE_COMMAND = "remove";

        public static string GetPlaylistName(string PlaylistName)
        { 
            return PlaylistName == FAVORITE_PLAYLIST_NAME ? FAVORITE_PLAYLIST_DISPLAY_NAME : PlaylistName;
        }
    }
}
