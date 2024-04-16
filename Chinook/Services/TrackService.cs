using AutoMapper;
using Chinook.ClientModels;
using Chinook.Interfaces;

namespace Chinook.Services
{
    public class TrackService : ITrackService
    {
        private ITrackRepository TrackRepository;
        private IMapper Mapper;

        public TrackService(ITrackRepository trackRepository, IMapper Mapper)
        {
            this.TrackRepository = trackRepository;
            this.Mapper = Mapper;
        }

        /// <summary>
        /// The method retrieves Playlist tracks according to given artist.
        /// </summary>
        /// <param name="ArtistId"></param>
        /// <param name="CurrentUserId"></param>
        /// <returns></returns>
        public async Task<List<PlaylistTrack>> GetPlaylistTracksAsync(long ArtistId, string CurrentUserId)
        {
            var Tracks = await TrackRepository.GetAllTracksAsync(ArtistId);
            return Mapper.Map<List<PlaylistTrack>>(Tracks, opt => opt.Items["UserId"] = CurrentUserId);
        }
    }
}
