using Chinook.ClientModels;
using Chinook.Interfaces;

namespace Chinook.Services
{
    public class TrackService : ITrackService
    {
        private ITrackRepository trackRepository;

        public TrackService(ITrackRepository trackRepository)
        {
            this.trackRepository = trackRepository;
        }

        public async Task<List<PlaylistTrack>> GetPlaylistTracksAsync(long ArtistId, string CurrentUserId)
        {
            var tracks = await trackRepository.GetAllTracksAsync(ArtistId);
            return tracks.Select(t => new PlaylistTrack()
            {
                AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                TrackId = t.TrackId,
                TrackName = t.Name,
                IsFavorite = t.Playlists.Any(p => p.UserId == CurrentUserId && p.Name == "Favorites")
            }).ToList();
        }
    }
}
