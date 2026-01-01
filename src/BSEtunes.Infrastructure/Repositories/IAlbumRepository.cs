using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IAlbumRepository
    {
        Task<AlbumEntity?> GetAlbumByIdAsync(int albumId);

        Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail);
    }
}
