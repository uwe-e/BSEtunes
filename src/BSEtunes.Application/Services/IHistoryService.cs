using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface IHistoryService
    {
        Task<HistoryEntity> AddPlayHistoryAsync(HistoryEntity history);
        Task<HistoryEntity?> GetHistoryByIdAsync(int id);
        Task<IEnumerable<HistoryEntity>> GetUserHistoryAsync(string owner, int limit = 50);
        Task<IEnumerable<HistoryEntity>> GetRecentHistoryAsync(int limit = 50);
    }
}