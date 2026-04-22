using AutoMapper;
using BSEtunes.Contracts.DTOs.Playlists;
using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Mapping
{
    public class PlaylistProfile : Profile
    {
        public PlaylistProfile()
        {
            CreateMap<PlaylistSummaryEntity, PlaylistDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Owner, opt => opt.MapFrom(src => src.Owner))
                .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Guid))
                .ForMember(dest => dest.EntryCount, opt => opt.MapFrom(src => src.EntryCount));

            CreateMap<PlaylistEntryEntity, PlaylistEntryDto>()
                .ForMember(dest => dest.Track, opt => opt.MapFrom(src => src.Track));

            CreateMap<CreatePlaylistDto, PlaylistEntity>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
            
            CreateMap<PlaylistEntity, PlaylistSummaryDto>();
        }
    }
}
