using AutoMapper;
using Chinook.ClientModels;
using Chinook.Interfaces;

namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository ArtistRepository;
        private readonly IMapper Mapper;

        public ArtistService(IArtistRepository ArtistRepository, IMapper Mapper)
        {
            this.ArtistRepository = ArtistRepository;
            this.Mapper = Mapper;   
        }

        /// <summary>
        /// The method retuns all artist details maps to artist view model.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Artist>> GetAllArtistsAsync()
        {
            var ArtistList = await ArtistRepository.GetAllArtistsAsync();
            return Mapper.Map<List<Artist>>(ArtistList);
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
            return Mapper.Map<Artist>(Artist);
        }


    }
}
