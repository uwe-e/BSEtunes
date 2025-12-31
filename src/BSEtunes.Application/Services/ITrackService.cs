using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface ITrackService
    {
        Task<TrackEntity?> GetTrackByIdAsync(int id);
        Task<IList<int>?> GetTrackIdsByFilter(int? genreId);
    }
}
