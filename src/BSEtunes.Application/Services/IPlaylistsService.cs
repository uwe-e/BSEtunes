using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface IPlaylistsService
    {
        Task AppendPlaylistEntriesAsync(int playlistId, List<int> trackIds);

        Task<PlaylistSummaryEntity> CreatePlaylistAsync(PlaylistEntity playlist);

        Task<bool> DeletePlaylistAsync(int playlistId, string owner);

        Task<int> DeletePlaylistEntriesAsync(int playlistId, List<int> entryIds, string owner);

        Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(
            string owner,
            int pageNumber = 1,
            int pageSize = 10);

        Task<PlaylistSummaryEntity?> GetPlaylistByOwnerAndIdAsync(string owner, int playlistId);

        Task<PagedResult<PlaylistEntryEntity>> GetPagedPlaylistEntriesByIdAsync(
            int playlistId,
            string owner,
            int pageNumber,
            int pageSize);

        Task<IReadOnlyList<PlaylistEntity>> GetPlaylistsByOwnerAsync(string owner);

        Task<List<int>> GetTrackIdsByPlaylistIdAsync(int playlistId, bool randomize = false);
        Task<bool> ReorderPlaylistEntriesAsync(int playlistId, List<int> entryIds, string owner);
    }
}
