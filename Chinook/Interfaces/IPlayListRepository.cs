using Chinook.Models;

namespace Chinook.Interfaces
{
    public interface IPlayListRepository
    {
        //The method returns palylist according to playlist id.
        Task<Playlist?> GetPlayListAsync(long PlayListId);
        // The method returns all the playlists according to user.
        Task<List<Playlist>> GetAllPlayListsAsync(string UserId);

        /// <summary>
        /// The method only retrun playlist that equals to PlayListName.
        /// </summary>
        /// <param name="PlaylistName"></param>
        /// <returns></returns>
        Task<Playlist?> GetPlaylistByNameAsync(string UserId, string PlaylistName);
        // The method creates a new playlist.
        Task<Playlist> CreatePlaylistAsync(Playlist PlayList);
        //The method remove the playlist accroding to given playlist id.
        Task RemovePlaylistAsync(long PlaylistId);
        // The method update update the playlist and the Tracks according to update command.
        Task UpdatePlaylistAsync(Playlist Playlist, string UpdateCommand);


    }
}
