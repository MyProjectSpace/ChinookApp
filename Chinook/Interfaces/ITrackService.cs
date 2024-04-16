using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Interfaces
{
    public interface ITrackService
    {
        // The service method returns list of play list tracks according to artist id.
        Task<List<PlaylistTrack>> GetPlaylistTracksAsync(long artistId, string currentUserId);
        
    }
}
