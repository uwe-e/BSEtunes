using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class SearchService(ISearchRepository searchRepository) : ISearchService
    {
        public Task<PagedResult<AlbumEntity>> GetAlbumSearchAsync(string searchPhrase, int pageSize, int pageIndex)
        {
           return searchRepository.GetAlbumSearchAsync(searchPhrase, pageSize, pageIndex);
        }

        public Task<PagedResult<TrackEntity>> GetTrackSearchAsync(string searchPhrase, int pageSize, int pageIndex)
        {
            return searchRepository.GetTrackSearchAsync(searchPhrase, pageSize, pageIndex);
        }
    }
}
