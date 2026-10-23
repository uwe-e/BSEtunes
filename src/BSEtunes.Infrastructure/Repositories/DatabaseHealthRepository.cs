using BSEtunes.Infrastructure.Data;

namespace BSEtunes.Infrastructure.Repositories
{
    public class DatabaseHealthRepository : IDatabaseHealthRepository
    {
        private readonly RecordsDbContext _context;

        public DatabaseHealthRepository(RecordsDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Checks if the database is accessible and ready.
        /// </summary>
        /// <returns>True if the database is accessible, otherwise false.</returns>
        public async Task<bool> IsDatabaseAccessibleAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}
