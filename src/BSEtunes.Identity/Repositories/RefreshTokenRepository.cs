using BSEtunes.Identity.Data;
using BSEtunes.Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace BSEtunes.Identity.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly IdentityDbContext _context;

        public RefreshTokenRepository(IdentityDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task SaveAsync(RefreshToken token)
        {
            if (token == null) throw new ArgumentNullException(nameof(token));
            _context.RefreshTokens.Add(token);
            return _context.SaveChangesAsync();
        }

        public async Task RevokeTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentException("token is required", nameof(token));

            // Use a server-side update to avoid loading the entity into memory and reduce tracker overhead.
            // ExecuteUpdateAsync performs a direct SQL UPDATE and is efficient for single-field updates.
            await _context.RefreshTokens
                .Where(t => t.Token == token && !t.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true));
        }

        public Task<RefreshToken?> ValidateRefreshTokenAsync(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return Task.FromResult<RefreshToken?>(null);

            // Read-only lookup: use AsNoTracking to avoid change-tracking cost when only validating.
            return _context.RefreshTokens
                .AsNoTracking()
                .SingleOrDefaultAsync(t => t.Token == token);
        }
    }
}
