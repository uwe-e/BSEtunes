using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BSEtunes.Infrastructure.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly RecordsDbContext _context;

        public AlbumRepository(RecordsDbContext context)
        {
            _context = context;
        }
        public async Task<AlbumEntity?> GetAlbumByIdAsync(int albumId)
        {
            var dbAlbums = await _context.Albums
                .FirstOrDefaultAsync(a => a.Album_Id == albumId);

            if (dbAlbums is null)
                return null;

            var dbLieder = await _context.Tracks
                .Where(t => t.AlbumId == albumId)
                .OrderBy(t => t.TrackNumber)
                .ToListAsync();

            return AlbumMapper.ToDomain(dbAlbums, dbLieder);
        }
    }
}
