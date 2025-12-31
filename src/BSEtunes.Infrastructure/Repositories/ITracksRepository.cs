using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface ITracksRepository
    {
        Task<TrackEntity?> GetTrackByIdAsync(int id);
        Task<IList<int>?> GetTrackIdsByFilter(int? genreId);
    }
}
