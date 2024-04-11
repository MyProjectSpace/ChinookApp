using AutoMapper;
using Chinook.ClientModels;
using Chinook.Exceptions;
using Chinook.Interfaces;
using Chinook.Shared.Common;
using System.Reactive.Joins;

namespace Chinook.Services
{
    public class PlayListService : IPlayListService
    {
        private IPlayListRepository PlayListRepository;
        private IMapper Mapper;

        public PlayListService(IPlayListRepository playListRepository, IMapper Mapper)
        {
            PlayListRepository = playListRepository;
            this.Mapper = Mapper;
        }

        /// <summary>
        /// The method creates new playlist according to user id.
        /// </summary>
        /// <param name="NewPlaylist"></param>
        /// <param name="UserId"></param>
        /// <returns>Returns newly create Playlist</returns>
        /// <exception cref="ChinookException"></exception>
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

            var Playlist = Mapper.Map<Models.Playlist>(NewPlaylist);
            Mapper.Map<string, Models.Playlist>(UserId, Playlist);
            var NewlyCreatedPlaylist = await PlayListRepository.CreatePlaylistAsync(Playlist);
            return Mapper.Map<Playlist>(NewlyCreatedPlaylist);

        }

        /// <summary>
        /// The method updates tracks in playlist.
        /// </summary>
        /// <param name="Playlist"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        /// <exception cref="ChinookException"></exception>
        public async Task UpdateTracksInPlaylistAsync(Playlist Playlist, string UserId)
        {
            if (Playlist == null || string.IsNullOrEmpty(UserId))
            {
                throw new ChinookException("Invalid playlist details or user id");
            }

            var PlayList = Mapper.Map<Models.Playlist>(Playlist, opt => opt.Items["UserId"] = UserId);
            if (await IsInValidPlaylistAsync(UserId, Playlist))
            {
                throw new ChinookException("Playlist name already exists");
            }
            await PlayListRepository.UpdatePlaylistAsync(PlayList, ConstantName.ADD_COMMAND);
        }


        /// <summary>
        /// The method retrieves all playlist for given user id.
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns>List of Playlists returns</returns>
        public async Task<List<Playlist>> GetAllPlaylistsAsync(string UserId)
        {
            var PlayLists = await PlayListRepository.GetAllPlayListsAsync(UserId);
            return Mapper.Map<List<ClientModels.Playlist>>(PlayLists, opt => opt.Items["UserId"] = UserId);
        }

        /// <summary>
        /// The method returns given playlist according to playlist id.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PlayListId"></param>
        /// <returns></returns>
        /// <exception cref="KeyNotFoundException"></exception>
        public async Task<Playlist> GetPlayListAsync(string UserId, long PlayListId)
        {
            var PlayList = await PlayListRepository.GetPlayListAsync(PlayListId);
            if (PlayList == null)
            {
                throw new KeyNotFoundException("Invalid playlist");
            }
            return Mapper.Map<ClientModels.Playlist>(PlayList, opt => opt.Items["UserId"] = UserId);

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
            var FavoritePlaylist = await PlayListRepository.GetPlaylistByNameAsync(UserId, ConstantName.FAVORITE_PLAYLIST_NAME);
            var Playlist = new Models.Playlist()
            {
                UserId = UserId,
                Name = ConstantName.FAVORITE_PLAYLIST_NAME,
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
            return Mapper.Map<ClientModels.Playlist>(Playlist);
        }

        /// <summary>
        /// The method only removes given track from Favorite playlist using track id.
        /// If the removing track id is the only track in the Favorite playlist after remove track, it deletes playlist as well.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="TrackId"></param>
        /// <returns></returns>
        public async Task RemoveTrackFromUserFavoritePlaylistAsync(string UserId, long TrackId)
        {
            var FavoritePlaylist = await PlayListRepository.GetPlaylistByNameAsync(UserId, ConstantName.FAVORITE_PLAYLIST_NAME);
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
                }, ConstantName.REMOVE_COMMAND);

                if (FavoritePlaylist.Tracks.Count == 1)
                {
                    await PlayListRepository.RemovePlaylistAsync(FavoritePlaylist.PlaylistId);
                }
            }
        }

        /// <summary>
        /// The method removes the given track using track id.
        /// If the given track is the only track in the playlist it will remove the track as well.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PlaylistId"></param>
        /// <param name="TrackId"></param>
        /// <returns></returns>
        /// <exception cref="ChinookException"></exception>
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
                }, ConstantName.REMOVE_COMMAND);
                if (Playlist.Tracks.Count == 1)
                {
                    await PlayListRepository.RemovePlaylistAsync(Playlist.PlaylistId);
                }
            }
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

        public async Task<Playlist> GetUserFavoritePlaylistAsync(string UserId)
        {
            var FavoritePlaylist = await PlayListRepository.GetPlaylistByNameAsync(UserId, ConstantName.FAVORITE_PLAYLIST_NAME);
            return Mapper.Map<ClientModels.Playlist>(FavoritePlaylist);
        }
    }
}
