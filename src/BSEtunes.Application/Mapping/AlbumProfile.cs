using AutoMapper;
using BSEtunes.Contracts.DTOs.Albums;
using BSEtunes.Contracts.DTOs.Playlists;
using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Mapping
{
    public class AlbumProfile : Profile
    {
        public AlbumProfile()
        {
            CreateMap<TrackEntity, TrackDto>()
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Guid.ToString()))
                .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.Extension))
                .ForMember(dest => dest.TrackNumber, opt => opt.MapFrom(src => src.TrackNumber))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            
            CreateMap<AlbumEntity, AlbumDto>()
                .ForMember(dest => dest.Artist, opt => opt.MapFrom(src => 
                    src.Artist != null ? new ArtistDto
                    {
                        Id = src.Artist.Id,
                        Name = src.Artist.Name,
                        SortName = src.Artist.SortName
                    } : null))
                .ForMember(dest => dest.Genre, opt => opt.MapFrom(src => 
                    src.Genre != null ? new GenreDto
                    {
                        Id = src.Genre.Id,
                        Name = src.Genre.Name
                    } : null));
                //.ForMember(dest => dest.Tracks, opt => opt.MapFrom(src => new TrackDto
                //{
                //    Id = src.Track.Id,
                //    Name = src.Tracks.Name,
                //    TrackNumber = src.Tracks.TrackNumber,
                //    Duration = src.Tracks.Duration,
                //    Guid = src.Tracks.Guid,
                //    Extension = src.Tracks.Extension
                //}));
            
            CreateMap<AlbumDto, AlbumEntity>()
                .ForMember(dest => dest.Tracks, opt => opt.Ignore())
                .ForMember(dest => dest.Artist, opt => opt.Ignore());
            //CreateMap<Infrastructure.Models.Track, Domain.Entities.TrackEntity>()
            //    .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration.HasValue ? src.Duration.Value.TimeOfDay : TimeSpan.Zero))
            //    .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => Guid.Parse(src.Guid)))
            //    .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => System.IO.Path.GetExtension(src.Liedpfad ?? string.Empty).TrimStart('.')))
            //    .ForMember(dest => dest.TrackNumber, opt => opt.MapFrom(src => src.TrackNumber ?? 0))
            //    .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name ?? string.Empty));
            CreateMap<PlaylistSummaryEntity, PlaylistDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner))
                .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Guid))
                .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount));

            CreateMap<PlaylistEntryEntity, PlaylistEntryDto>()
                .ForMember(dest => dest.Track, opt => opt.MapFrom(src => src.Track));
        }
    }
}
