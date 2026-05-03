using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.Services
{
    public interface IGenreService
    {
        Task<IReadOnlyList<GenreEntity>> GetAvailableGenresAsync();
    }
}