using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IPlaylistsRepository
    {
        Task AppendPlaylistEntriesAsync(int playlistId, List<int> trackIds);

        Task<PlaylistSummaryEntity> CreatePlaylistAsync(PlaylistEntity playlist);

        Task<bool> DeletePlaylistAsync(int playlistId, string owner);

        Task<bool> DeletePlaylistEntryAsync(int playlistId, int entryId, string owner);

        Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(
            string owner,
            int pageNumber,
            int pageSize);
        Task<PlaylistSummaryEntity?> GetPlaylistByOwnerAndIdAsync(string owner, int playlistId);

        Task<PagedResult<PlaylistEntryEntity>> GetPagedPlaylistEntriesByIdAsync(
            int playlistId,
            string owner,
            int pageNumber,
            int pageSize);

        Task<IReadOnlyList<PlaylistEntity>> GetPlaylistsByOwnerAsync(string owner);

        Task<List<int>> GetTrackIdsByPlaylistIdAsync(int playlistId, bool randomize = false);
    }
}
