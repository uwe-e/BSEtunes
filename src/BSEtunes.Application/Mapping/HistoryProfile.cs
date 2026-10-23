using AutoMapper;
using BSEtunes.Contracts.DTOs.History;
using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Mapping
{
    public class HistoryProfile : Profile
    {
        public HistoryProfile()
        {
            CreateMap<HistoryEntity, HistoryDto>();
            CreateMap<CreateHistoryDto, HistoryEntity>();
        }
    }
}