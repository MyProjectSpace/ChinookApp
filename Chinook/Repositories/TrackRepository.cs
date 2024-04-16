using Chinook.ClientModels;
using Chinook.Interfaces;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Repositories
{
    /// <summary>
    /// This class handle Track related db operations.
    /// </summary>
    public class TrackRepository : ITrackRepository
    {
        private IDbContextFactory<ChinookContext> DbFactory;

        public TrackRepository(IDbContextFactory<ChinookContext> dbFactory)
        {
            DbFactory = dbFactory;
        }

        /// <summary>
        /// The method returns all tracks for a given artist id.
        /// </summary>
        /// <param name="artistId"></param>
        /// <returns></returns>
        public async Task<List<Track>> GetAllTracksAsync(long artistId)
        {
            var Dbcontext = await DbFactory.CreateDbContextAsync();
            var Artist = Dbcontext.Artists.SingleOrDefault(a => a.ArtistId == artistId);
            //Exception is thrown to indicate invalid artist id. If an artist is not available should not proceed.
            if (Artist == null)
            {
                throw new KeyNotFoundException("Invalid artist");
            }

            return Dbcontext.Tracks.Where(a => a.Album.ArtistId == artistId)
                .Include(a => a.Album)
                .Include(p => p.Playlists)
                .ToList();
        }
    }
}
