using BSEtunes.Contracts.Enums;
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

        public async Task<IEnumerable<AlbumEntity>> GetSortedAlbumsAsync(AlbumSortOption sortBy = AlbumSortOption.Random, int limit = 10)
        {
            return await _repository.GetSortedAlbumsAsync(sortBy, limit);
        }

        public async Task<PagedResult<AlbumEntity>> GetPagedAlbumsAsync(
            AlbumFilterOptions? filterOptions = null,
            AlbumSortOption sortBy = AlbumSortOption.Random,
            int pageNumber = 1,
            int pageSize = 10)
        {
            return await _repository.GetPagedAlbumsAsync(filterOptions, sortBy, pageNumber, pageSize);
        }
    }
}
