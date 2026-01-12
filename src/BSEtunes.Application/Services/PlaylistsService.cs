using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class PlaylistsService(IPlaylistsRepository repository) : IPlaylistsService
    {
        public Task<PagedResult<PlaylistEntryEntity>> GetPagedPlaylistEntriesByIdAsync(int playlistId, string owner, int pageNumber, int pageSize)
        {
            return repository.GetPagedPlaylistEntriesByIdAsync(playlistId, owner, pageNumber, pageSize);
        }

        public async Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(string owner, int pageNumber = 1, int pageSize = 10)
        {
            return await repository.GetPagedPlaylistsByOwnerAsync(owner, pageNumber, pageSize);
        }

        public async Task<PlaylistSummaryEntity?> GetPlaylistByOwnerAndIdAsync(string owner, int playlistId)
        {
            return await repository.GetPlaylistByOwnerAndIdAsync(owner, playlistId);
        }
    }
}
