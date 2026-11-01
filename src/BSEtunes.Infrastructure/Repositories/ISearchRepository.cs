using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface ISearchRepository
    {
        Task<PagedResult<AlbumEntity>> GetAlbumSearchAsync(string searchPhrase, int pageSize, int pageIndex);
        Task<IList<string>?> GetSuggestionsAsync(string query, int limit);
        Task<PagedResult<TrackEntity>> GetTrackSearchAsync(string searchPhrase, int pageSize, int pageIndex);
    }
}