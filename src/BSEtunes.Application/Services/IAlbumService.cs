using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface IAlbumService
    {
        Task<AlbumEntity?> GetAlbumByIdAsync(int albumId);
        Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail);
    }
}
