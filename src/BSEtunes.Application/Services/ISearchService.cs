using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface ISearchService
    {
        Task<PagedResult<AlbumEntity>> GetAlbumSearchAsync(string searchPhrase, int pageSize, int pageIndex);
        Task<PagedResult<TrackEntity>> GetTrackSearchAsync(string searchPhrase, int pageSize, int pageIndex);
    }
}
