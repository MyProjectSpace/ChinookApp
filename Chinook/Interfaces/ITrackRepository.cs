using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Interfaces
{
    public interface ITrackRepository
    {
        // Get all tracks by artist id
        Task<List<Track>> GetAllTracksAsync(long artistId);

    }
}
