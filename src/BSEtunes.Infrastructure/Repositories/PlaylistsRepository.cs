using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BSEtunes.Infrastructure.Repositories
{
    public class PlaylistsRepository : IPlaylistsRepository
    {
        private readonly RecordsDbContext _context;
        private readonly ILogger<PlaylistsRepository> _logger;

        public PlaylistsRepository(RecordsDbContext context, ILogger<PlaylistsRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(string owner, int pageNumber = 1, int pageSize = 10)
        {
            var totalStopwatch = Stopwatch.StartNew();

            IQueryable<PlaylistSummary> query = _context.PlaylistSummaries
                .Where(p => p.Owner == owner);

            // Get total count
            var countStopwatch = Stopwatch.StartNew();
            var totalCount = await query.CountAsync();
            countStopwatch.Stop();
            _logger.LogDebug("Count query took {ElapsedMs}ms for owner {Owner}",
                countStopwatch.ElapsedMilliseconds, owner);


            // Apply sorting and pagination
            var playlistsStopwatch = Stopwatch.StartNew();
            var playlists = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            playlistsStopwatch.Stop();
            _logger.LogDebug("Playlists query took {ElapsedMs}ms, returned {Count} items",
                playlistsStopwatch.ElapsedMilliseconds, playlists.Count);

            // Get playlist IDs for album lookup
            var playlistIds = playlists.Select(p => p.Id).ToList();

            // Get first 4 album IDs for each playlist
            var albumIdsStopwatch = Stopwatch.StartNew();

            var albumIds = await _context.PlaylistEntries
                .Where(pe => playlistIds.Contains(pe.PlaylistId))
                .GroupBy(pe => pe.PlaylistId)
                .Select(g => new
                {
                    PlaylistId = g.Key,
                    AlbumIds = g.OrderBy(pe => pe.SortOrder)
                        .Take(4)
                        .Select(pe => _context.Tracks
                            .Where(t => t.Id == pe.TrackId)
                            .Join(_context.Titles,
                                t => t.AlbumId,
                                title => title.Id,
                                (t, title) => title.Guid)
                            .FirstOrDefault())
                        .Distinct()
                        .ToList()
                })
                .ToListAsync();
            albumIdsStopwatch.Stop();
            _logger.LogDebug("Album IDs query took {ElapsedMs}ms", albumIdsStopwatch.ElapsedMilliseconds);

            // Create lookup dictionary
            var albumIdLookup = albumIds.ToDictionary(a => a.PlaylistId, a => a.AlbumIds);

            return new PagedResult<PlaylistSummaryEntity>
            {
                Items = playlists.Select(p => new PlaylistSummaryEntity
                {
                    Id = p.Id,
                    Name = p.Name,
                    Owner = p.Owner,
                    Guid = p.Guid,
                    EntryCount = (int)p.EntryCount,
                    CoverAlbumIds = albumIdLookup.ContainsKey(p.Id)
                        ? albumIdLookup[p.Id].Where(id => id != null).Select(id => id!).ToList()
                        : new List<string>()
                }),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PlaylistSummaryEntity?> GetPlaylistByOwnerAndIdAsync(string owner, int playlistId)
        {
            var totalStopwatch = Stopwatch.StartNew();

            // Get playlist summary
            var playlistStopwatch = Stopwatch.StartNew();
            var playlist = await _context.PlaylistSummaries
                .Where(p => p.Owner == owner && p.Id == playlistId)
                .FirstOrDefaultAsync();
            playlistStopwatch.Stop();
            _logger.LogDebug("Playlist query took {ElapsedMs}ms for owner {Owner} and playlistId {PlaylistId}",
                playlistStopwatch.ElapsedMilliseconds, owner, playlistId);

            if (playlist == null)
            {
                return null;
            }

            // Get first 4 album IDs for the playlist
            var albumIdsStopwatch = Stopwatch.StartNew();
            var albumIds = await _context.PlaylistEntries
                .Where(pe => pe.PlaylistId == playlistId)
                .OrderBy(pe => pe.SortOrder)
                .Take(4)
                .Select(pe => _context.Tracks
                    .Where(t => t.Id == pe.TrackId)
                    .Join(_context.Titles,
                        t => t.AlbumId,
                        title => title.Id,
                        (t, title) => title.Guid)
                    .FirstOrDefault())
                .Distinct()
                .ToListAsync();
            albumIdsStopwatch.Stop();
            _logger.LogDebug("Album IDs query took {ElapsedMs}ms", albumIdsStopwatch.ElapsedMilliseconds);

            totalStopwatch.Stop();
            _logger.LogDebug("Total query took {ElapsedMs}ms", totalStopwatch.ElapsedMilliseconds);

            return new PlaylistSummaryEntity
            {
                Id = playlist.Id,
                Name = playlist.Name,
                Owner = playlist.Owner,
                Guid = playlist.Guid,
                EntryCount = (int)playlist.EntryCount,
                CoverAlbumIds = albumIds.Where(id => id != null).Select(id => id!).ToList()
            };
        }
    }
}
