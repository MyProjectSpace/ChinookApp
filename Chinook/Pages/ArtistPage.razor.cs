using Chinook.ClientModels;
using Chinook.Shared.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Chinook.Interfaces;
using Chinook.Exceptions;

namespace Chinook.Pages
{
    public partial class ArtistPage
    {

        [Parameter] public long ArtistId { get; set; }
        [CascadingParameter] public EventCallback<Playlist> OnPlaylistAddOrUpdate { get; set; }
        [CascadingParameter] private Task<AuthenticationState> AuthenticationState { get; set; }
        [Inject] ITrackService TrackService { get; set; }
        [Inject] IArtistService ArtistService { get; set; }
        [Inject] ILogger<ArtistPage> Logger { get; set; }
        [Inject] IPlayListService PlayListService { get; set; }

        private Modal PlaylistDialog { get; set; }
        private ClientModels.Artist Artist;
        private List<PlaylistTrack> Tracks;
        private PlaylistTrack SelectedTrack;
        private string InfoMessage;
        private string CurrentUserId;
        private string ErrorMessage;
        private long PlaylistId = -1;
        private List<Playlist> Playlists = new();
        private string NewPlayListName;
        private string ModalErrorMessage;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                await InvokeAsync(StateHasChanged);
                CurrentUserId = await GetUserId();
                Artist = await ArtistService.GetArtistAsync(ArtistId);
                Tracks = await TrackService.GetPlaylistTracksAsync(ArtistId, CurrentUserId);
                await LoadPlaylistDropdown();
            }
            catch (KeyNotFoundException kex)
            {
                ErrorMessage = kex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, $"Contact Admin.Failed loading artist details.");
            }
        }

        private async Task LoadPlaylistDropdown()
        {
            Playlists = await PlayListService.GetAllPlaylistsAsync(CurrentUserId);
            Playlists.Insert(0, new Playlist()
            {
                Name = "--Select Playlist--",
                PlaylistId = -1,
                Tracks = new List<PlaylistTrack>()
            });
        }

        private async Task<string> GetUserId()
        {
            var user = (await AuthenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async Task FavoriteTrack(long TrackId)
        {
            try
            {
                var Track = Tracks.Single(t => t.TrackId == TrackId);
                var Playlist = await PlayListService.AddTrackToUserFavoritePlaylistAsync(CurrentUserId, TrackId);
                Track.IsFavorite = true;
                InfoMessage = $"Track {Track.ArtistName} - {Track.AlbumTitle} - {Track.TrackName} added to playlist Favorites.";
                await OnPlaylistAddOrUpdate.InvokeAsync(Playlist);
            }
            catch (ChinookException cex)
            {
                ErrorMessage = cex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Contact Admin.Failed making the track favorite");
                ErrorMessage = "Contact Admin to resolve issue";
            }
        }

        private async void UnfavoriteTrack(long TrackId)
        {
            try
            {
                var track = Tracks.FirstOrDefault(t => t.TrackId == TrackId);
                await PlayListService.RemoveTrackFromUserFavoritePlaylistAsync(CurrentUserId, TrackId);
                Tracks.Single(t => t.TrackId == TrackId).IsFavorite = false;
                InfoMessage = $"Track {track.ArtistName} - {track.AlbumTitle} - {track.TrackName} removed from playlist Favorites.";
            }
            catch (ChinookException cex)
            {
                ErrorMessage = cex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed removing track from favorite list");
                ErrorMessage = "Contact Admin to resolve issue";
            }
        }

        private void OpenPlaylistDialog(long TrackId)
        {
            CloseInfoMessage();
            SelectedTrack = Tracks.FirstOrDefault(t => t.TrackId == TrackId);
            PlaylistDialog.Open();
        }

        private async Task AddTrackToPlaylist()
        {
            // When PlaylistId is -1 it is considered as existing playlist is not selected and new playlist is created.
            // When existing playlist is not selected, new playlist name is required.
            try
            {
                ClientModels.Playlist Playlist = new ClientModels.Playlist();
                if (SelectedTrack != null)
                {

                    if (PlaylistId != -1)
                    {
                        Playlist = GetPlaylistForUpdate(PlaylistId);
                        await PlayListService.UpdateTracksInPlaylistAsync(Playlist, CurrentUserId);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(NewPlayListName))
                        {
                            ModalErrorMessage = "Name for the new playlist is required";
                            StateHasChanged();
                            return;
                        }
                        Playlist = GetNewPlaylist(PlaylistId, NewPlayListName);
                        var CreatedPlaylist = await PlayListService.CreatePlaylitAsync(Playlist, CurrentUserId);
                        Playlist.PlaylistId = CreatedPlaylist.PlaylistId;
                        // Adding newly create playlist to Playlists to reflect it locally in popup.
                        Playlists.Add(Playlist);
                        // Notifying new playlist added and show it in Navbar
                        await OnPlaylistAddOrUpdate.InvokeAsync(Playlist);
                    }
                }
                else
                {
                    ModalErrorMessage = "Invalid selected track";
                }
                var PlaylistName = PlaylistId == -1 ? NewPlayListName : Playlists.Single(p => p.PlaylistId == PlaylistId).Name;
                ClosePlaylistModal();
                PlaylistDialog.Close();
                InfoMessage = $"Track {Artist.Name} - {SelectedTrack.AlbumTitle} - {SelectedTrack.TrackName} added to playlist {PlaylistName}.";
                
            }
            catch (ChinookException cex)
            {
                ModalErrorMessage = cex.Message;
            }
            catch (DbUpdateException dex)
            {
                Logger.LogError(dex, "Failed creating playlist");
                ModalErrorMessage = "Contact admin. Create or update failed";
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Unexpected exception. Create or update failed ");
                ModalErrorMessage = "Contact admin. Create or update failed";
            }
        }

        private Playlist GetNewPlaylist(long PlaylistId, string PlaylistName)
        {
            return new Playlist()
            {
                Name = PlaylistName,
                Tracks = new List<PlaylistTrack>() { SelectedTrack }
            };

        }

        private Playlist GetPlaylistForUpdate(long PlaylistId)
        {
            return new Playlist()
            {
                PlaylistId = PlaylistId,
                Tracks = new List<PlaylistTrack>() { SelectedTrack },
            };
        }

        private void CloseInfoMessage()
        {
            InfoMessage = "";
        }

        private void CloseErrorMessage()
        {
            ErrorMessage = "";
        }

        private void CloseModalErrorMessage()
        {
            ModalErrorMessage = "";
        }

        private void ClosePlaylistModal()
        {
            PlaylistId = -1;
            ModalErrorMessage = "";
            NewPlayListName = "";
            CloseModalErrorMessage();
        }

    }

}
