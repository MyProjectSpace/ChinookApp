using Chinook.ClientModels;

namespace Chinook.Interfaces
{
    public interface IArtistService
    {
        //The service method to get artist details by artist id.
        Task<Artist> GetArtistAsync(long ArtistId);
        // The service method to get all the artists.
        Task<List<Artist>> GetAllArtistsAsync();
    }
}
