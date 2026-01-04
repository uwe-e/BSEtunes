using BSEtunes.Domain.Entities;
using BSEtunes.Domain.Enums;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IAlbumRepository
    {
        Task<AlbumEntity?> GetAlbumByIdAsync(int albumId);
        Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail);
        Task<IEnumerable<AlbumEntity>> GetFeaturedAlbumsAsync(int limit);
        Task<IEnumerable<AlbumEntity>> GetSortedAlbumsAsync(AlbumSortOption sortBy = AlbumSortOption.Random, int limit = 10);
    }
}
