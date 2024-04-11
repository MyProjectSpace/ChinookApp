using Chinook.ClientModels;

namespace Chinook.Interfaces
{
    public interface IArtistService
    {
        Task<Artist> GetArtistAsync(long ArtistId);
        Task<List<Artist>> GetAllArtistsAsync();
    }
}
