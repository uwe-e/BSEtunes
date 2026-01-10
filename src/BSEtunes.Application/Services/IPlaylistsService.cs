using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface IPlaylistsService
    {
        Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(
            string owner,
            int pageNumber = 1,
            int pageSize = 10);

        Task<PlaylistSummaryEntity?> GetPlaylistByOwnerAndIdAsync(string owner, int playlistId);
    }
}
