using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IPlaylistsRepository
    {
        Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(
            string owner,
            int pageNumber = 1,
            int pageSize = 10);
        Task<PlaylistSummaryEntity?> GetPlaylistByOwnerAndIdAsync(string owner, int playlistId);
    }
}
