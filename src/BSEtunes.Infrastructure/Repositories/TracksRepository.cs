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
        /// <summary>
        /// Asynchronously retrieves the number of tracks that have an associated file path.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the number of tracks with a
        /// non-null file path.</returns>
        public async Task<int> GetAvailableTrackCountAsync()
        {
            return await _context.Tracks
                .Where(t => t.FilePath != null)
                .CountAsync();
        }
        /// <summary>
        /// Asynchronously retrieves a track by its unique identifier, including related album and artist information.
        /// </summary>
        /// <param name="id">The unique identifier of the track to retrieve.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the track entity with associated
        /// album and artist data if found; otherwise, null.</returns>
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
        /// <summary>
        /// Asynchronously retrieves a list of track IDs that have a file path and optionally match the specified genre.
        /// </summary>
        /// <param name="genreId">The identifier of the genre to filter tracks by. If null, tracks from all genres are included.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a list of track IDs that match
        /// the filter criteria, or null if no tracks are found.</returns>
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
