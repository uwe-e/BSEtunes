using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class HistoryService : IHistoryService
    {
        private readonly IHistoryRepository _repository;

        public HistoryService(IHistoryRepository repository)
        {
            _repository = repository;
        }

        public async Task<HistoryEntity> AddPlayHistoryAsync(HistoryEntity history)
        {
            // Set PlayedAt to current time if not specified
            if (history.PlayedAt == default)
            {
                history.PlayedAt = DateTime.UtcNow;
            }

            return await _repository.AddAsync(history);
        }

        public async Task<HistoryEntity?> GetHistoryByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<HistoryEntity>> GetUserHistoryAsync(string owner, int limit = 50)
        {
            return await _repository.GetByOwnerAsync(owner, limit);
        }

        public async Task<IEnumerable<HistoryEntity>> GetRecentHistoryAsync(int limit = 50)
        {
            return await _repository.GetRecentAsync(limit);
        }
    }
}