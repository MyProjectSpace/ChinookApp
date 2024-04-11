using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Interfaces
{
    public interface ITrackService
    {
        Task<List<PlaylistTrack>> GetPlaylistTracksAsync(long artistId, string currentUserId);
        
    }
}
