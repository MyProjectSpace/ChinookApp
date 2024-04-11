using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Chinook.Interfaces;
using Chinook.Services;
using Chinook.Exceptions;
using NuGet.DependencyResolver;
using Chinook.Shared.Components;
using Chinook.ClientModels;

namespace Chinook.Pages
{
    public partial class PlaylistPage
    {
        [Parameter] public long PlaylistId { get; set; }
        [CascadingParameter] public EventCallback<Playlist> OnPlaylistAddOrUpdate { get; set; }
        [Inject] IPlayListService PlaylistService { get; set; }
        [Inject] ILogger<PlaylistPage> Logger { get; set; }
        [Inject] NavigationManager NavigationManager { get; set; } = null!;
        [CascadingParameter] private Task<AuthenticationState> authenticationState { get; set; }
        private Modal DeleteConfirmationDialog { get; set; }

        private Chinook.ClientModels.Playlist Playlist;
        private PlaylistTrack TrackToDelete;
        private string CurrentUserId;
        private string InfoMessage;
        private string ErrorMessage;
        private string DeleteDialogErrorMessage;
        //private long TrackIdToDelete;


        protected override async Task OnParametersSetAsync()
        {
            try
            {
                CurrentUserId = await GetUserId();
                Playlist = await PlaylistService.GetPlayListAsync(CurrentUserId, PlaylistId);
                await base.OnParametersSetAsync();
            }
            catch (KeyNotFoundException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (ChinookException cex)
            {
                ErrorMessage = cex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed loading playlist data");
                ErrorMessage = "Contact Admin. Failed loading playlist data";
            }
        }

        private async Task<string> GetUserId()
        {
            var user = (await authenticationState).User;
            var userId = user.FindFirst(u => u.Type.Contains(ClaimTypes.NameIdentifier))?.Value;
            return userId;
        }

        private async Task FavoriteTrack(long TrackId)
        {
            try
            {
                var Track = Playlist.Tracks.Single(t => t.TrackId == TrackId);
                var FavoritePlaylist = await PlaylistService.AddTrackToUserFavoritePlaylistAsync(CurrentUserId, TrackId);
                Track.IsFavorite = true;
                InfoMessage = $"Track {Track.ArtistName} - {Track.AlbumTitle} - {Track.TrackName} added to playlist Favorites.";
                await OnPlaylistAddOrUpdate.InvokeAsync(FavoritePlaylist);
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

        private async Task UnfavoriteTrack(long TrackId)
        {
            try
            {
                var track = Playlist.Tracks.FirstOrDefault(t => t.TrackId == TrackId);
                await PlaylistService.RemoveTrackFromUserFavoritePlaylistAsync(CurrentUserId, TrackId);
                Playlist.Tracks.Single(t => t.TrackId == TrackId).IsFavorite = false;
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

        private async Task RemoveTrack(long TrackId)
        {
            try
            {
                await PlaylistService.RemoveTrackFromPlaylistAsync(CurrentUserId, PlaylistId, TrackId);
                var Track = Playlist.Tracks.Single(t => t.TrackId == TrackId);
                if (Playlist.Tracks.Count == 1)
                {
                    NavigationManager.NavigateTo("/", true);
                }
                else
                {
                    Playlist.Tracks.Remove(Track);
                    InfoMessage = $"Track {Track.ArtistName} - {Track.AlbumTitle} - {Track.TrackName} removed from playlist.";
                }
                TrackToDelete = null;
                DeleteConfirmationDialog.Close();

            }
            catch (ChinookException cex)
            {
                DeleteDialogErrorMessage = cex.Message;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Removing track from playlist");
                DeleteDialogErrorMessage = "Contact Admin. Failed removing track";
            }


        }

        private void ShowDeleteDialog(PlaylistTrack Track)
        {
            TrackToDelete = Track;
            DeleteConfirmationDialog.Open();
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
            DeleteDialogErrorMessage = "";
        }

    }
}
