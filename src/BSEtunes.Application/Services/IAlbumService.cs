using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface IAlbumService
    {
        Task<AlbumEntity?> GetAlbumByIdAsync(int albumId);
    }
}
