using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;

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

        public async Task<CoverImageEntity?> GetAlbumCoverImageAsync(Guid albumId, bool asThumbnail)
        {
            var title = await _context.Titles
                .Where(t => t.Guid == albumId.ToString())
                .Select(t => new
                {
                    t.Extension,
                    Image = asThumbnail ? t.Thumbnail : t.Cover,
                    t.MutationDate
                })
                .FirstOrDefaultAsync();

            if (title?.Extension == null || title.Image == null)
                return null;

            return new CoverImageEntity
            {
                Blob = title.Image,
                Extension = title.Extension,
                LastModified = title.MutationDate
            };
        }

        public async Task<IEnumerable<AlbumEntity>> GetFeaturedAlbumsAsync(int limit = 10)
        {
            var albums = await _context.Albums
                .OrderByDescending(a => EF.Functions.Random())
                .Take(limit)
                .ToListAsync();
            return AlbumMapper.ToDomain(albums) ?? Enumerable.Empty<AlbumEntity>();
        }
    }
}
