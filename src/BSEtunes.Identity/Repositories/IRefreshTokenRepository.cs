using BSEtunes.Identity.Models;

namespace BSEtunes.Identity.Repositories
{
    public interface IRefreshTokenRepository
    {
        Task SaveAsync(RefreshToken token);
        Task<RefreshToken?> ValidateRefreshTokenAsync(string token);
        Task RevokeTokenAsync(string token);
    }
}
