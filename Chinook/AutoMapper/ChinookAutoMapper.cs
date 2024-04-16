using AutoMapper;
using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Pages;
using Chinook.Shared.Common;

namespace Chinook.AutoMapper
{
    public class ChinookAutoMapper : Profile
    {
        public ChinookAutoMapper() 
        {
            CreateMap<ClientModels.PlaylistTrack, Track>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.TrackName))
                .ForMember(dest => dest.TrackId, opt => opt.MapFrom(src => src.TrackId));

            CreateMap<Models.Track, ClientModels.PlaylistTrack>()
                .ForMember(dest => dest.TrackName, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TrackId, opt => opt.MapFrom(src => src.TrackId))
                .ForMember(dest => dest.AlbumTitle, opt => opt.MapFrom(src => (src.Album != null ? src.Album.Title : "-")))
                .ForMember(dest => dest.ArtistName, opt => opt.MapFrom(src => (src.Album != null && src.Album.Artist != null) ? src.Album.Artist.Name : "-"))
                // This mapping checks if the playlists are created by the given user id and the playlist contains Favorite name, it marks as a Favorite track.
                .ForMember(dest => dest.IsFavorite, opt => opt.MapFrom((src,dest,destMember, context) => src.Playlists.Any(p =>( p.UserId == (context.TryGetItems(out var Items) ? (string)Items["UserId"] : null) && p.Name == ConstantName.FAVORITE_PLAYLIST_NAME))));

            CreateMap<Models.Playlist, ClientModels.Playlist>()
                .ForMember(dest => dest.PlaylistId, opt => opt.MapFrom(src => src.PlaylistId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => src.Tracks));

            CreateMap<ClientModels.Playlist, Models.Playlist>()
               .ForMember(dest => dest.PlaylistId, opt => opt.MapFrom(src => src.PlaylistId))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => src.Tracks));

            CreateMap<long, ClientModels.Playlist>()
                .ForMember(dest => dest.PlaylistId, opt => opt.MapFrom(src => src));

            CreateMap<string, Models.Playlist>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src));

            CreateMap<Models.Artist, ClientModels.Artist>()
                .ForMember(dest => dest.ArtistId, opt => opt.MapFrom(src => src.ArtistId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.AlbumsCount, opt => opt.MapFrom(src => src.Albums.Count));


        }
    }
}
