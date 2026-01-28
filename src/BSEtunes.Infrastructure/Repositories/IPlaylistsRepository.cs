using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IPlaylistsRepository
    {
        Task<PlaylistSummaryEntity> CreatePlaylistAsync(PlaylistEntity playlist);

        Task<bool> DeletePlaylistAsync(int playlistId, string owner);

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

        Task<List<int>> GetTrackIdsByPlaylistIdAsync(int playlistId, bool randomize = false);
    }
}
