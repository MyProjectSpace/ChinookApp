using Chinook.Models;

namespace Chinook.Interfaces
{
    public interface IArtistRepository
    {
        //The method return Artist according to ArtistId.
        Task<Artist?> GetArtistAsync(long ArtistId);
        //The method return list of all artists.
        Task<List<Artist>> GetAllArtistsAsync();
    }
}
