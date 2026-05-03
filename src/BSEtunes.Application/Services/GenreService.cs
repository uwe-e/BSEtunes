using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Repositories;

namespace BSEtunes.Application.Services
{
    public class GenreService : IGenreService
    {
        private readonly IGenreRepository _genreRepository;

        public GenreService(IGenreRepository genreRepository)
        {
            _genreRepository = genreRepository;
        }

        public Task<IReadOnlyList<GenreEntity>> GetAvailableGenresAsync()
        {
            return _genreRepository.GetAvailableGenresAsync();
        }
    }
}