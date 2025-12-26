namespace BSEtunes.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for checking database health and connectivity.
    /// </summary>
    public interface IDatabaseHealthRepository
    {
        /// <summary>
        /// Checks if the database is accessible and ready.
        /// </summary>
        /// <returns>True if the database is accessible, otherwise false.</returns>
        Task<bool> IsDatabaseAccessibleAsync();
    }
}
