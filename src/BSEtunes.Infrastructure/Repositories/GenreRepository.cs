using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BSEtunes.Infrastructure.Repositories
{
    public class GenreRepository : IGenreRepository
    {
        private readonly RecordsDbContext _context;

        public GenreRepository(RecordsDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<GenreEntity>> GetAvailableGenresAsync()
        {
            // Only genres that are referenced by at least one album
            var genreIds = await _context.Albums
                .Select(a => a.Genre_Id)
                .Distinct()
                .ToListAsync();

            var genres = await _context.Genres
                .Where(g => genreIds.Contains(g.Id))
                .Select(g => new GenreEntity
                {
                    Id = g.Id,
                    Name = g.Name
                })
                .OrderBy(g => g.Name)
                .ToListAsync();

            return genres;
        }
    }
}