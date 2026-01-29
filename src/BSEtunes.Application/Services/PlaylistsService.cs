using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class PlaylistsService(IPlaylistsRepository repository) : IPlaylistsService
    {
        public Task AppendPlaylistEntriesAsync(int playlistId, List<int> trackIds)
        {
            return repository.AppendPlaylistEntriesAsync(playlistId, trackIds);
        }

        public Task<PlaylistSummaryEntity> CreatePlaylistAsync(PlaylistEntity playlist)
        {
            return repository.CreatePlaylistAsync(playlist);
        }

        public Task<bool> DeletePlaylistAsync(int playlistId, string owner)
        {
            return repository.DeletePlaylistAsync(playlistId, owner);
        }

        public Task<bool> DeletePlaylistEntryAsync(int playlistId, int entryId, string owner)
        {
            return repository.DeletePlaylistEntryAsync(playlistId, entryId, owner);
        }

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

        public async Task<List<int>> GetTrackIdsByPlaylistIdAsync(int playlistId, bool randomize = false)
        {
            return await repository.GetTrackIdsByPlaylistIdAsync(playlistId, randomize);
        }
    }
}
