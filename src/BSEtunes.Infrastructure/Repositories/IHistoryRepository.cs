using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IHistoryRepository
    {
        Task<HistoryEntity> AddAsync(HistoryEntity history);
        Task<HistoryEntity?> GetByIdAsync(int id);
        Task<IEnumerable<HistoryEntity>> GetByOwnerAsync(string owner, int limit = 50);
        Task<IEnumerable<HistoryEntity>> GetRecentAsync(int limit = 50);
    }
}