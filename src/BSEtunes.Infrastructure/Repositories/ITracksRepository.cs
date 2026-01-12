using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface ITracksRepository
    {
        Task<int> GetAvailableTrackCountAsync();
        Task<TrackEntity?> GetTrackByIdAsync(int id);
        Task<IList<int>?> GetTrackIdsByFilter(int? genreId);
        Task<IList<TrackEntity>> GetTracksByIdsAsync(IEnumerable<int> ids);
    }
}
