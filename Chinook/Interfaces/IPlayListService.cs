using Chinook.ClientModels;
using System.Threading.Tasks;

namespace Chinook.Interfaces
{
    public interface IPlayListService
    {
        //The service method to get playlist according to playlist id and tracks.
        // The tracks will should if they are favorites or not.
        Task<Playlist> GetPlayListAsync(string UserId, long PlayListId);

        //The method returns all the playlists according to user id.
        Task<List<Playlist>> GetAllPlaylistsAsync(string UserId);
        
        // The method creates a new playlist for a User.
        Task<Playlist> CreatePlaylitAsync(Playlist NewPlaylist, string UserId);

        // The service method to update tracks in a playlist.
        Task UpdateTracksInPlaylistAsync(Playlist Playlist, string UserId);
        
        // The service method to add tracks to user favorite playlist.
        Task<Playlist> AddTrackToUserFavoritePlaylistAsync(string UserId, long TrackId);

        // The service method to remove track from the user favorite playlist.
        Task RemoveTrackFromUserFavoritePlaylistAsync(string UserId, long TrackId);

        // The service method to remove a trac from the given playlist.
        Task RemoveTrackFromPlaylistAsync(string UserId, long PlaylistId, long TrackId);

        Task<Playlist> GetUserFavoritePlaylistAsync(string UserId);


    }
}
