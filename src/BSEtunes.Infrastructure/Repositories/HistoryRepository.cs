using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;

namespace BSEtunes.Infrastructure.Repositories
{
    public class HistoryRepository : IHistoryRepository
    {
        private readonly RecordsDbContext _context;

        public HistoryRepository(RecordsDbContext context)
        {
            _context = context;
        }

        public async Task<HistoryEntity> AddAsync(HistoryEntity history)
        {
            var dbHistory = HistoryMapper.ToDatabase(history);
            
            _context.Histories.Add(dbHistory);
            await _context.SaveChangesAsync();
            
            return HistoryMapper.ToDomain(dbHistory);
        }

        public async Task<HistoryEntity?> GetByIdAsync(int id)
        {
            var dbHistory = await _context.Histories
                .FirstOrDefaultAsync(h => h.Id == id);

            return dbHistory == null ? null : HistoryMapper.ToDomain(dbHistory);
        }

        public async Task<IEnumerable<HistoryEntity>> GetByOwnerAsync(string owner, int limit = 50)
        {
            var dbHistories = await _context.Histories
                .Where(h => h.Owner == owner)
                .OrderByDescending(h => h.PlayedAt)
                .Take(limit)
                .ToListAsync();

            return dbHistories.Select(HistoryMapper.ToDomain);
        }

        public async Task<IEnumerable<HistoryEntity>> GetRecentAsync(int limit = 50)
        {
            var dbHistories = await _context.Histories
                .OrderByDescending(h => h.PlayedAt)
                .Take(limit)
                .ToListAsync();

            return dbHistories.Select(HistoryMapper.ToDomain);
        }
    }
}