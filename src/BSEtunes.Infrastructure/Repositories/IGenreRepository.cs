using BSEtunes.Domain.Entities;

namespace BSEtunes.Infrastructure.Repositories
{
    public interface IGenreRepository
    {
        Task<IReadOnlyList<GenreEntity>> GetAvailableGenresAsync();
    }
}