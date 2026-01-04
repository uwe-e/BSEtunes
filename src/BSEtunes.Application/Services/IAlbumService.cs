using BSEtunes.Domain.Entities;
using BSEtunes.Domain.Enums;

namespace BSEtunes.Application.Services
{
    public interface IAlbumService
    {
        Task<AlbumEntity?> GetAlbumByIdAsync(int albumId);
        Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail);
        Task<IEnumerable<AlbumEntity>> GetSortedAlbumsAsync(
            AlbumSortOption sortBy = AlbumSortOption.Random, int limit = 10);
        Task<PagedResult<AlbumEntity>> GetPagedAlbumsAsync(
            AlbumFilterOptions? filterOptions = null,
            AlbumSortOption sortBy = AlbumSortOption.Random,
            int pageNumber = 1,
            int pageSize = 10);
    }
}
