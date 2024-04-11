using Chinook.ClientModels;
using Chinook.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Chinook.Pages
{
    public partial class Home
    {
        private List<Artist> Artists;
        private List<Artist> UnFilteredArtists;
        private string ErrorMessage = string.Empty;
        private string SearchString { get; set; } = string.Empty;
        [Inject] IArtistService ArtistService { get; set; }
        [Inject] ILogger<Home> Logger { get; set; }
        protected override async Task OnInitializedAsync()
        {
            try
            {
                await InvokeAsync(StateHasChanged);
                UnFilteredArtists = Artists = await ArtistService.GetAllArtistsAsync();
            }
            // Since this is unforseen exception log error details and ask end user to contact admin to resolve issue.
            catch (Exception ex)
            {
                Logger.LogError(ex, "Contact Admin.Failed loading artists");

            }
        }



        // This method will not search artists in database. Since all the artists are loaded to the front end at the moments.
        // If there are considerable amount of artists are there we should retrieve using pagination from the database.
        private void SearchArtist()
        {

            if (!SearchString.IsNullOrEmpty())
            {
                Artists = UnFilteredArtists.Where(a => a.Name.Contains(SearchString, StringComparison.InvariantCultureIgnoreCase)).ToList();
            }
            else
            {
                Artists = UnFilteredArtists;
            }
        }

        private void CloseErrorMessage()
        {
            ErrorMessage = "";
        }
    }
}
