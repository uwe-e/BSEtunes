using BSEtunes.Contracts.Enums;
using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IAlbumRepository
    {
        Task<AlbumEntity?> GetAlbumByIdAsync(int albumId);
        Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail);
        Task<PagedResult<AlbumEntity>> GetPagedAlbumsAsync(
                    AlbumFilterOptions? filterOptions = null,
                    AlbumSortOption sortBy = AlbumSortOption.Random,
                    int pageNumber = 1,
                    int pageSize = 10);
        Task<IEnumerable<AlbumEntity>> GetSortedAlbumsAsync(
            AlbumSortOption sortBy = AlbumSortOption.Random, int limit = 10);
        Task<PagedResult<TrackEntity>> GetPagedTracksByAlbumIdAsync(
            int albumId,
            int pageNumber = 1,
            int pageSize = 20);
    }
}
