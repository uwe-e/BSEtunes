using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class TrackService : ITrackService
    {
        private readonly ITracksRepository _tracksRepository;

        public TrackService(ITracksRepository tracksRepository )
        {
            _tracksRepository = tracksRepository;
        }

        public async Task<int> GetAvailableTrackCountAsync()
        {
            return await _tracksRepository.GetAvailableTrackCountAsync();
        }

        public async Task<TrackEntity?> GetTrackByGuidAsync(Guid guid)
        {
            return await _tracksRepository.GetTrackByGuidAsync(guid);
        }

        public async Task<TrackEntity?> GetTrackByIdAsync(int id)
        {
            return await _tracksRepository.GetTrackByIdAsync(id);
        }

        public async Task<IList<int>?> GetTrackIdsByFilter(int? genreId)
        {
            return await _tracksRepository.GetTrackIdsByFilter(genreId);
        }
    }
}
