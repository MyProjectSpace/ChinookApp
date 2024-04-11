using Chinook.Exceptions;
using Chinook.Interfaces;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging;

namespace Chinook.Repositories
{
    public class PlayListRepository : IPlayListRepository
    {
        private IDbContextFactory<ChinookContext> DbFactory;
        private const string FAVORITE_PLAYLIST_NAME = "Favorites";
        public PlayListRepository(IDbContextFactory<ChinookContext> dbFactory)
        {
            DbFactory = dbFactory;
        }

        public async Task<Playlist> CreatePlaylistAsync(Playlist PlayList)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            await AddTracksToNewPlaylistAsync(PlayList, DbContext);
            await GetUserDetailsAsync(PlayList, DbContext);
            DbContext.Playlists.Add(PlayList);
            await DbContext.SaveChangesAsync();
            return PlayList;
        }

        public async Task<List<Playlist>> GetAllPlayListsAsync(string UserId)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Playlists
                .Include(p => p.Tracks).ThenInclude(a => a.Album).ThenInclude(ar => ar.Artist)
                .Where(u => u.UserId == UserId)
                .ToListAsync();
        }

        public async Task<Playlist?> GetPlayListAsync(long PlaylistId)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Playlists
                .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                .Include(a => a.Tracks).ThenInclude(a => a.Playlists)
                .FirstOrDefaultAsync(p => p.PlaylistId == PlaylistId);
        }

        public async Task<Playlist> GetPlaylistByNameAsync(string UserId, string PlaylistName)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Playlists.Include(a => a.Tracks)
                .ThenInclude(t => t.Album).ThenInclude(a => a.Artist)
                .SingleOrDefaultAsync(p => p.UserId == UserId && p.Name == PlaylistName);
        }

        public async Task<Playlist> GetUserFavoritePlaylistAsync(string UserId)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            return await DbContext.Playlists.Include(p => p.Tracks).SingleOrDefaultAsync(p => p.UserId == UserId && p.Name == FAVORITE_PLAYLIST_NAME);
        }

        public async Task UpdateTracksInPlaylistAsync(Playlist Playlist)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            var ExistingPlaylist = await DbContext.Playlists.Include(t => t.Tracks).SingleOrDefaultAsync(p => p.PlaylistId == Playlist.PlaylistId);
            if (ExistingPlaylist == null)
            {
                throw new ChinookException("Invalid playlist");
            }
            await AddTracksToExistingPlaylistAsync(ExistingPlaylist, Playlist.Tracks.ToList(), DbContext);

            await DbContext.SaveChangesAsync();
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

        public async Task UpdatePlaylistAsync(Playlist Playlist, string mode)
        {
            using var DbContext = await DbFactory.CreateDbContextAsync();
            var ExistingPlaylist = await DbContext.Playlists.Include(t => t.Tracks)
                .Include(t => t.User)
                .SingleOrDefaultAsync(p => p.PlaylistId == Playlist.PlaylistId);
            if (ExistingPlaylist == null)
            {
                throw new ChinookException("Invalid playlist");
            }

            if (mode.ToLower() == "add")
            {
               await AddTracksToExistingPlaylistAsync(ExistingPlaylist, Playlist.Tracks.ToList(), DbContext);
            }
            else
            {
                await RemoveTrackFromPlaylistAsync(ExistingPlaylist, Playlist.Tracks.ToList(), DbContext);
            }
            await DbContext.SaveChangesAsync();
        }

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

        private async Task GetUserDetailsAsync(Playlist NewPlaylist, ChinookContext DbContext)
        {
            var User = await DbContext.Users.FindAsync(NewPlaylist.UserId);
            if (User == null)
            {
                throw new ChinookException("Invalid user, create a valid user");
            }
            NewPlaylist.User = User;

        }

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
