using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _repository;

        public AlbumService(IAlbumRepository repository)
        {
            _repository = repository;
        }
        public async Task<AlbumEntity?> GetAlbumByIdAsync(int albumId)
        {
            return await _repository.GetAlbumByIdAsync(albumId);
        }

        public async Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail)
        {
            return await _repository.GetAlbumCoverImageAsync(albumId, asThumbnail);
        }

        public async Task<IEnumerable<AlbumEntity>> GetFeaturedAlbumsAsync(int limit)
        {
            return await _repository.GetFeaturedAlbumsAsync(limit);
        }
    }
}
