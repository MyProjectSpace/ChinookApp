using Chinook.ClientModels;
using Chinook.Interfaces;

namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository ArtistRepository;

        public ArtistService(IArtistRepository artistRepository)
        {
            ArtistRepository = artistRepository;
        }

        /// <summary>
        /// The method retuns all artist details maps to artist view model.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Artist>> GetAllArtistsAsync()
        {
            var ArtistList = await ArtistRepository.GetAllArtistsAsync();
            return ArtistList != null ? ArtistList.Select(a => new Artist()
            {
                Name = a.Name,
                AlbumsCount = a.Albums.Count,
                ArtistId = a.ArtistId,
            }).ToList() : new List<Artist>();
        }

        /// <summary>
        /// This method returns artist view model details by given artist id.
        /// </summary>
        /// <param name="ArtistId"></param>
        /// <returns></returns>
        public async Task<Artist> GetArtistAsync(long ArtistId)
        {
            var Artist = await ArtistRepository.GetArtistAsync(ArtistId);
            if (Artist == null)
            {
                throw new KeyNotFoundException("Invalid artist");
            }
            return new ClientModels.Artist()
            {
                Name = Artist.Name
            };
        }


    }
}
