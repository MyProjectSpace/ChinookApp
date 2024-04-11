using Chinook.Models;

namespace Chinook.Interfaces
{
    public interface IArtistRepository
    {
        Task<Artist> GetArtistAsync(long ArtistId);
        Task<List<Artist>> GetAllArtistsAsync();
    }
}
