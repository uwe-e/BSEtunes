using BSEtunes.Domain.Entities;
using BSEtunes.Domain.Enums;

namespace BSEtunes.Application.Services
{
    public interface IAlbumService
    {
        Task<AlbumEntity?> GetAlbumByIdAsync(int albumId);
        Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail);
        Task<IEnumerable<AlbumEntity>> GetFeaturedAlbumsAsync(int limit);
        Task<IEnumerable<AlbumEntity>> GetSortedAlbumsAsync(AlbumSortOption sortBy = AlbumSortOption.Random, int limit = 10);
    }
}
