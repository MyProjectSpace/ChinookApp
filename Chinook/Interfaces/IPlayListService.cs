using Chinook.ClientModels;
using System.Threading.Tasks;

namespace Chinook.Interfaces
{
    public interface IPlayListService
    {
        Task<Playlist> GetPlayListAsync(string UserId, long PlayListId);
        Task<List<Playlist>> GetAllPlaylistsAsync(string UserId);
        Task<Playlist> CreatePlaylitAsync(Playlist NewPlaylist, string UserId);
        Task UpdateTracksInPlaylistAsync(Playlist Playlist, string UserId);
        Task<Playlist> AddTrackToUserFavoritePlaylistAsync(string UserId, long TrackId);
        Task RemoveTrackFromUserFavoritePlaylistAsync(string UserId, long TrackId);
        Task RemoveTrackFromPlaylistAsync(string UserId, long PlaylistId, long TrackId);


    }
}
