using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Mapping;
using Microsoft.EntityFrameworkCore;

namespace BSEtunes.Infrastructure.Repositories
{
    public class TracksRepository : ITracksRepository
    {
        private readonly RecordsDbContext _context;

        public TracksRepository(RecordsDbContext context)
        {
            _context = context;
        }

        public async Task<TrackEntity?> GetTrackByIdAsync(int id)
        {
            var result = await _context.Tracks
                .Where(t => t.Id == id)
                .Join(_context.Albums, t => t.AlbumId, a => a.Album_Id, (t, a) => new { Track = t, Album = a })
                .Join(_context.Artists, ta => ta.Album.Artist_Id, ar => ar.Id, (ta, ar) => new { ta.Track, ta.Album, Artist = ar })
                .Select(x => new { x.Track, x.Album, x.Artist })
                .FirstOrDefaultAsync();

            return result != null ? TrackMapper.ToDomain(result.Track, result.Album, result.Artist) : null;
        }

        public async Task<IList<int>?> GetTrackIdsByFilter(int? genreId)
        {
            var query = _context.Tracks
                .Where(t => t.FilePath != null);

            if (genreId.HasValue)
            {
                query = query
                    .Join(_context.Albums, t => t.AlbumId, a => a.Album_Id, (t, a) => new { Track = t, Album = a })
                    .Where(x => x.Album.Genre_Id == genreId.Value)
                    .Select(x => x.Track);
                //query = query
                //    .Join(_context.Albums, t => t.AlbumId, a => a.Album_Id, (t, a) => new { t.Id, a.Genre_Id })
                //    .Where(x => x.Genre_Id == genreId.Value)
                //    .Select(x => x.Id);
            }
            //else
            //{
            //    query = query.Select(t => t.Id);
            //}

            return await query.Select(t => t.Id).ToListAsync();
        }
    }
}
