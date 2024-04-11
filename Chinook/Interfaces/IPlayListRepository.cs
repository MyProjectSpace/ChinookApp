using Chinook.Models;

namespace Chinook.Interfaces
{
    public interface IPlayListRepository
    {
        Task<Playlist> GetPlayListAsync(long PlayListId);
        Task<List<Playlist>> GetAllPlayListsAsync(string UserId);

        /// <summary>
        /// The method only retrun playlist that equals to PlayListName.
        /// </summary>
        /// <param name="PlaylistName"></param>
        /// <returns></returns>
        Task<Playlist> GetPlaylistByNameAsync(string UserId, string PlaylistName);
        Task<Playlist> CreatePlaylistAsync(Playlist PlayList);
        Task RemovePlaylistAsync(long PlaylistId);
        Task UpdatePlaylistAsync(Playlist Playlist, string mode);


    }
}
