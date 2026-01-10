using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class PlaylistsService : IPlaylistsService
    {
        private readonly IPlaylistsRepository _repository;

        public PlaylistsService(IPlaylistsRepository repository)
        {
            _repository = repository;
        }
        public async Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(string owner, int pageNumber = 1, int pageSize = 10)
        {
            return await _repository.GetPagedPlaylistsByOwnerAsync(owner, pageNumber, pageSize);
        }

        public async Task<PlaylistSummaryEntity?> GetPlaylistByOwnerAndIdAsync(string owner, int playlistId)
        {
            return await _repository.GetPlaylistByOwnerAndIdAsync(owner, playlistId);
        }
    }
}
