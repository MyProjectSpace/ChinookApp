using Chinook.Interfaces;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Repositories
{
    /// <summary>
    /// This class handles Artist related db operations.
    /// </summary>
    public class ArtistRepository : IArtistRepository
    {
        private IDbContextFactory<ChinookContext> DbFactory;
        public ArtistRepository(IDbContextFactory<ChinookContext> dbFactory)
        {
            DbFactory = dbFactory;
        }

        /// <summary>
        /// Get Artist details by artist id.
        /// </summary>
        /// <param name="ArtistId"></param>
        /// <returns>If valid artist id artist details return otherwise null</returns>
        public async Task<Artist?> GetArtistAsync(long ArtistId)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Artists.SingleOrDefaultAsync(a => a.ArtistId == ArtistId);
        }

        /// <summary>
        /// The method returns all artists details.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Artist>> GetAllArtistsAsync()
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            var albumList = await DbContext.Artists.Include(a => a.Albums).ToListAsync();
            return albumList;

        }
    }
}
