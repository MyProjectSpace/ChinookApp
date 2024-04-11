using Chinook.ClientModels;
using Chinook.Exceptions;
using Chinook.Interfaces;
using System.Reactive.Joins;

namespace Chinook.Services
{
    public class PlayListService : IPlayListService
    {
        private IPlayListRepository PlayListRepository;
        private const string FAVORITE_PLAYLIST_NAME = "Favorites";
        private const string FAVORITE_PLAYLIST_DISPLAY_NAME = "My favorite tracks";

        public PlayListService(IPlayListRepository playListRepository)
        {
            PlayListRepository = playListRepository;
        }

        public async Task<Playlist> CreatePlaylitAsync(Playlist NewPlaylist, string UserId)
        {
            if (NewPlaylist == null)
            {
                throw new ChinookException("Invalid playlist details");
            }

            if (await IsInValidPlaylistAsync(UserId, NewPlaylist))
            {
                throw new ChinookException("Playlist name already exists");
            }

            var Playlist = GetMappedDbPlaylist(NewPlaylist, UserId);
            var NewlyCreatedPlaylist = await PlayListRepository.CreatePlaylistAsync(Playlist);
            return new Playlist
            {
                PlaylistId = NewlyCreatedPlaylist.PlaylistId,
                Name = NewlyCreatedPlaylist.Name,
                Tracks = NewlyCreatedPlaylist.Tracks.Select(np => new PlaylistTrack()
                {
                    TrackId = np.TrackId,
                }).ToList()
            };

        }

        public async Task UpdateTracksInPlaylistAsync(Playlist Playlist, string UserId)
        {
            if (Playlist == null || string.IsNullOrEmpty(UserId))
            {
                throw new ChinookException("Invalid playlist details or user id");
            }

            var PlayList = new Models.Playlist()
            {
                Name = Playlist.Name,
                PlaylistId = Playlist.PlaylistId,
                Tracks = Playlist.Tracks.Select(t => new Models.Track
                {
                    TrackId = t.TrackId,
                    Name = t.TrackName
                }).ToList()
            };
            if (await IsInValidPlaylistAsync(UserId, Playlist))
            {
                throw new ChinookException("Playlist name already exists");
            }
            await PlayListRepository.UpdatePlaylistAsync(PlayList, "add");
        }

        private Models.Playlist GetMappedDbPlaylist(Playlist Playlist, string UserId)
        {
            return new Models.Playlist()
            {
                Name = Playlist.Name,
                PlaylistId = Playlist.PlaylistId,
                UserId = UserId,
                Tracks = Playlist.Tracks.Select(t => new Models.Track
                {
                    TrackId = t.TrackId,
                    Name = t.TrackName
                }).ToList(),
            };
        }

        // Method to validate playlist name at the moment. 
        //When there are more validation rules we can move this into seperate validator class.
        private async Task<bool> IsInValidPlaylistAsync(string UserId, Playlist NewPlaylist)
        {
            var PlayList = await PlayListRepository.GetPlaylistByNameAsync(UserId, NewPlaylist.Name);
            if (PlayList == null)
            {
                return false;
            }
            return true;
        }


        /// <summary>
        /// The method retrieves all playlist for given user id.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns>List of Playlists returns</returns>
        public async Task<List<Playlist>> GetAllPlaylistsAsync(string UserId)
        {
            var PlayLists = await PlayListRepository.GetAllPlayListsAsync(UserId);
            return PlayLists != null ? PlayLists.Select(pl => new Playlist()
            {
                Name = pl.Name == FAVORITE_PLAYLIST_NAME ? FAVORITE_PLAYLIST_DISPLAY_NAME : pl.Name,
                PlaylistId = pl.PlaylistId,
                Tracks = pl.Tracks.Select(t => new ClientModels.PlaylistTrack()
                {
                    AlbumTitle = t.Album != null ? t.Album.Title : string.Empty,
                    ArtistName = (t.Album != null && t.Album.Artist != null) ? t.Album.Artist.Name : string.Empty,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Any(p => p.Name == FAVORITE_PLAYLIST_NAME)
                }).ToList()

            }).ToList() : new List<Playlist>();
        }

        public async Task<Playlist> GetPlayListAsync(string UserId, long PlayListId)
        {
            var PlayList = await PlayListRepository.GetPlayListAsync(PlayListId);
            if (PlayList == null)
            {
                throw new KeyNotFoundException("Invalid playlist");
            }
            return new Playlist()
            {
                Name = PlayList.Name,
                Tracks = PlayList.Tracks.Select(t => new ClientModels.PlaylistTrack()
                {
                    AlbumTitle = t.Album != null ? t.Album.Title : string.Empty,
                    ArtistName = (t.Album != null && t.Album.Artist != null) ? t.Album.Artist.Name : string.Empty,
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Any(p => p.UserId == UserId && p.Name == FAVORITE_PLAYLIST_NAME)
                }).ToList()
            };
        }

        /// <summary>
        /// The method initially check if the favorite playlist is already created. If not favorite playlist is created.
        /// After creating playlist track will be mapped to the playlist or favorite playlist will be updated with the track.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="TrackId"></param>
        /// <returns></returns>
        public async Task<Playlist> AddTrackToUserFavoritePlaylistAsync(string UserId, long TrackId)
        {
            var FavoritePlaylist = await PlayListRepository.GetPlaylistByNameAsync(UserId, FAVORITE_PLAYLIST_NAME);
            var Playlist = new Models.Playlist()
            {
                UserId = UserId,
                Name = FAVORITE_PLAYLIST_NAME,
                Tracks = new List<Models.Track>() {
                        new Models.Track() {
                        TrackId = TrackId
                    } }
            };
            if (FavoritePlaylist == null)
            {
                await PlayListRepository.CreatePlaylistAsync(Playlist);
            }
            else
            {
                Playlist.PlaylistId = FavoritePlaylist.PlaylistId;
                await PlayListRepository.UpdatePlaylistAsync(Playlist, "add");
            }
            return GetMappedPlaylist(Playlist);
        }

        private Playlist GetMappedPlaylist(Models.Playlist PlaylistModel)
        {
            return new Playlist()
            {
                PlaylistId = PlaylistModel.PlaylistId,
                Name = PlaylistModel.Name,
                Tracks = PlaylistModel.Tracks != null ? PlaylistModel.Tracks.Select(t => new PlaylistTrack()
                {
                    TrackId = t.TrackId,
                    TrackName = t.Name
                }).ToList()
                : new List<PlaylistTrack>()
            };
        }

        public async Task RemoveTrackFromUserFavoritePlaylistAsync(string UserId, long TrackId)
        {
            var FavoritePlaylist = await PlayListRepository.GetPlaylistByNameAsync(UserId, FAVORITE_PLAYLIST_NAME);
            if (FavoritePlaylist != null && FavoritePlaylist.Tracks.Count > 0)
            {
                await PlayListRepository.UpdatePlaylistAsync(new Models.Playlist()
                {
                    PlaylistId = FavoritePlaylist.PlaylistId,
                    Tracks = new List<Models.Track>() {
                        { new Models.Track() {
                            TrackId = TrackId
                        } }
                    }
                }, "remove");

                if (FavoritePlaylist.Tracks.Count == 1)
                {
                    await PlayListRepository.RemovePlaylistAsync(FavoritePlaylist.PlaylistId);
                }
            }
        }

        public async Task RemoveTrackFromPlaylistAsync(string UserId, long PlaylistId, long TrackId)
        {
            var Playlist = await PlayListRepository.GetPlayListAsync(PlaylistId);
            if (Playlist == null)
            {
                throw new ChinookException("Invalid playlist");
            }

            if (Playlist != null && Playlist.Tracks.Count > 0)
            {
                await PlayListRepository.UpdatePlaylistAsync(new Models.Playlist()
                {
                    PlaylistId = PlaylistId,
                    Tracks = new List<Models.Track>()
                    {
                        new Models.Track() { TrackId = TrackId }
                    }
                }, "remove");
                if (Playlist.Tracks.Count == 1)
                {
                    await PlayListRepository.RemovePlaylistAsync(Playlist.PlaylistId);
                }
            }
        }


    }
}
