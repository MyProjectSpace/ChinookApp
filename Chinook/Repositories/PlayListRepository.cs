using Chinook.Exceptions;
using Chinook.Interfaces;
using Chinook.Models;
using Chinook.Shared.Common;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace Chinook.Repositories
{
    public class PlayListRepository : IPlayListRepository
    {
        private IDbContextFactory<ChinookContext> DbFactory;
        public PlayListRepository(IDbContextFactory<ChinookContext> dbFactory)
        {
            DbFactory = dbFactory;
        }

        /// <summary>
        /// The method create a new playlist.
        /// </summary>
        /// <param name="PlayList"></param>
        /// <returns></returns>
        public async Task<Playlist> CreatePlaylistAsync(Playlist PlayList)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            await AddTracksToNewPlaylistAsync(PlayList, DbContext);
            await GetUserDetailsAsync(PlayList, DbContext);
            DbContext.Playlists.Add(PlayList);
            await DbContext.SaveChangesAsync();
            return PlayList;
        }

        /// <summary>
        /// The method returns all playlists for a given userid.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<List<Playlist>> GetAllPlayListsAsync(string UserId)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(a => a.Album).ThenInclude(ar => ar.Artist)
                .Where(u => u.UserId == UserId)
                .ToListAsync();
        }

        /// <summary>
        /// The method returns Playlist according to given playlist id.
        /// </summary>
        /// <param name="PlaylistId"></param>
        /// <returns></returns>
        public async Task<Playlist?> GetPlayListAsync(long PlaylistId)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Playlists
                .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                .Include(a => a.Tracks).ThenInclude(a => a.Playlists)
                .FirstOrDefaultAsync(p => p.PlaylistId == PlaylistId);
        }

        /// <summary>
        /// The method returns a playlist according to given userid and playlist name.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PlaylistName"></param>
        /// <returns></returns>
        public async Task<Playlist?> GetPlaylistByNameAsync(string UserId, string PlaylistName)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Playlists.Include(a => a.Tracks)
                .ThenInclude(t => t.Album).ThenInclude(a => a.Artist)
                .SingleOrDefaultAsync(p => p.UserId == UserId && p.Name == PlaylistName);
        }

        public async Task RemovePlaylistAsync(long PlaylistId)
        { 
            using var DbContext = await DbFactory.CreateDbContextAsync();
            var Playlist = await DbContext.Playlists.Include(p => p.User).SingleOrDefaultAsync(p => p.PlaylistId == PlaylistId);
            if (Playlist == null)
            {
                throw new ChinookException("Invalid playlist");
            }
            DbContext.Playlists.Remove(Playlist);
            await DbContext.SaveChangesAsync();   
        }

        /// <summary>
        /// The method updates existing playlist.
        /// According to update command, the method add or remove tracks from the playlist.
        /// </summary>
        /// <param name="Playlist"></param>
        /// <param name="UpdateCommand"></param>
        /// <returns></returns>
        /// <exception cref="ChinookException"></exception>
        public async Task UpdatePlaylistAsync(Playlist Playlist, string UpdateCommand)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            var ExistingPlaylist = await DbContext.Playlists.Include(t => t.Tracks)
                .Include(t => t.User)
                .SingleOrDefaultAsync(p => p.PlaylistId == Playlist.PlaylistId);
            if (ExistingPlaylist == null)
            {
                throw new ChinookException("Invalid playlist");
            }

            // At this moment only tracks adding and removing happens.
            // If there are more modes we can go for factory pattern which such as TrackCommandFactory which provide TrackCommand.
            // Eg TrackCommand.Exectue will execute add, remove, etc commands.
            if (UpdateCommand.ToLower() == ConstantName.ADD_COMMAND)
            {
               await AddTracksToExistingPlaylistAsync(ExistingPlaylist, Playlist.Tracks.ToList(), DbContext);
            }
            else
            {
                await RemoveTrackFromPlaylistAsync(ExistingPlaylist, Playlist.Tracks.ToList(), DbContext);
            }
            await DbContext.SaveChangesAsync();
        }

        // The method tracks which Tracks to be removed from the playlist.
        private async Task RemoveTrackFromPlaylistAsync(Playlist ExistingPlaylist, List<Track> Tracks, ChinookContext DbContext)
        {
            foreach (var Track in Tracks)
            {
                var DbTrack = await DbContext.Tracks.FindAsync(Track.TrackId);
                if (DbTrack == null)
                {
                    throw new ChinookException("Invalid track");
                }
                ExistingPlaylist.Tracks.Remove(DbTrack);
            }
        }

        // The method fills the user property in the playlist.
        private async Task GetUserDetailsAsync(Playlist NewPlaylist, ChinookContext DbContext)
        {
            var User = await DbContext.Users.FindAsync(NewPlaylist.UserId);
            if (User == null)
            {
                throw new ChinookException("Invalid user, create a valid user");
            }
            NewPlaylist.User = User;

        }

        // The method adds tracks to new palylist.
        private async Task AddTracksToNewPlaylistAsync(Playlist NewPlaylist, ChinookContext DbContext)
        {
            List<Track> NewTracks = new List<Track>();
            foreach (var track in NewPlaylist.Tracks)
            {
                var Track = await DbContext.Tracks.FindAsync(track.TrackId);
                if (Track == null)
                {
                    throw new ChinookException("Invalid track");
                }
                NewTracks.Add(Track);
            }
            if (NewTracks.Count > 0)
            {
                NewPlaylist.Tracks = NewTracks;
            }
        }

        //The method add tracks to existing playlist.
        private async Task AddTracksToExistingPlaylistAsync(Playlist ExistingPlaylist, List<Track> Tracks, ChinookContext DbContext)
        {
            foreach (var track in Tracks)
            {
                var Track = await DbContext.Tracks.FindAsync(track.TrackId);
                if (Track == null)
                {
                    throw new ChinookException("Invalid track");
                }
                if (!ExistingPlaylist.Tracks.Any(t => t.TrackId == track.TrackId))
                {
                    ExistingPlaylist.Tracks.Add(Track);
                }
            }
        }

    }
}
