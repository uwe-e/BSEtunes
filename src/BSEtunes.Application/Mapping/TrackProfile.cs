using AutoMapper;
using BSEtunes.Contracts.DTOs.Albums;
using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Mapping
{
    public class TrackProfile : Profile
    {
        public TrackProfile() {
            
            CreateMap<TrackEntity, TrackDto>()
                   .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                   .ForMember(dest => dest.Guid, opt => opt.MapFrom(src => src.Guid.ToString()))
                   .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.Extension))
                   .ForMember(dest => dest.TrackNumber, opt => opt.MapFrom(src => src.TrackNumber))
                   .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
        }
    }
}
