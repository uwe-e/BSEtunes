using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BSEtunes.Infrastructure.Repositories
{
    public class PlaylistsRepository(
        RecordsDbContext context,
        ILogger<PlaylistsRepository> logger, ITracksRepository tracksRepository) : IPlaylistsRepository
    {
        public async Task<PagedResult<PlaylistSummaryEntity>> GetPagedPlaylistsByOwnerAsync(string owner, int pageNumber = 1, int pageSize = 10)
        {
            var totalStopwatch = Stopwatch.StartNew();

            IQueryable<PlaylistSummary> query = context.PlaylistSummaries
                .Where(p => p.Owner == owner);

            // Get total count
            var countStopwatch = Stopwatch.StartNew();
            var totalCount = await query.CountAsync();
            countStopwatch.Stop();
            logger.LogDebug("Count query took {ElapsedMs}ms for owner {Owner}",
                countStopwatch.ElapsedMilliseconds, owner);


            // Apply sorting and pagination
            var playlistsStopwatch = Stopwatch.StartNew();
            var playlists = await query
                .OrderBy(p => p.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            playlistsStopwatch.Stop();
            logger.LogDebug("Playlists query took {ElapsedMs}ms, returned {Count} items",
                playlistsStopwatch.ElapsedMilliseconds, playlists.Count);

            // Get playlist IDs for album lookup
            var playlistIds = playlists.Select(p => p.Id).ToList();

            // Get first 4 album IDs for each playlist
            var albumIdsStopwatch = Stopwatch.StartNew();

            var albumIds = await context.PlaylistEntries
                .Where(pe => playlistIds.Contains(pe.PlaylistId))
                .GroupBy(pe => pe.PlaylistId)
                .Select(g => new
                {
                    PlaylistId = g.Key,
                    AlbumIds = g.OrderBy(pe => pe.SortOrder)
                        .Take(4)
                        .Select(pe => context.Tracks
                            .Where(t => t.Id == pe.TrackId)
                            .Join(context.Titles,
                                t => t.AlbumId,
                                title => title.Id,
                                (t, title) => title.Guid)
                            .FirstOrDefault())
                        .Distinct()
                        .ToList()
                })
                .ToListAsync();
            albumIdsStopwatch.Stop();
            logger.LogDebug("Album IDs query took {ElapsedMs}ms", albumIdsStopwatch.ElapsedMilliseconds);

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
            var playlist = await context.PlaylistSummaries
                .Where(p => p.Owner == owner && p.Id == playlistId)
                .FirstOrDefaultAsync();
            playlistStopwatch.Stop();
            logger.LogDebug("Playlist query took {ElapsedMs}ms for owner {Owner} and playlistId {PlaylistId}",
                playlistStopwatch.ElapsedMilliseconds, owner, playlistId);

            if (playlist == null)
            {
                return null;
            }

            // Get first 4 album IDs for the playlist
            var albumIdsStopwatch = Stopwatch.StartNew();
            var albumIds = await context.PlaylistEntries
                .Where(pe => pe.PlaylistId == playlistId)
                .OrderBy(pe => pe.SortOrder)
                .Take(4)
                .Select(pe => context.Tracks
                    .Where(t => t.Id == pe.TrackId)
                    .Join(context.Titles,
                        t => t.AlbumId,
                        title => title.Id,
                        (t, title) => title.Guid)
                    .FirstOrDefault())
                .Distinct()
                .ToListAsync();
            albumIdsStopwatch.Stop();
            logger.LogDebug("Album IDs query took {ElapsedMs}ms", albumIdsStopwatch.ElapsedMilliseconds);

            totalStopwatch.Stop();
            logger.LogDebug("Total query took {ElapsedMs}ms", totalStopwatch.ElapsedMilliseconds);

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

        public async Task<PagedResult<PlaylistEntryEntity>> GetPagedPlaylistEntriesByIdAsync(
            int playlistId,
            string owner,
            int pageNumber = 1,
            int pageSize = 50)
        {
            // Verify ownership
            var playlistExists = await context.Playlists
                .AnyAsync(p => p.Id == playlistId && p.Owner == owner);

            if (!playlistExists)
            {
                return new PagedResult<PlaylistEntryEntity>
                {
                    Items = [],
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            // Get total count
            var totalCount = await context.PlaylistEntries
                .CountAsync(pe => pe.PlaylistId == playlistId);

            // Get playlist entries (without tracks)
            var entries = await context.PlaylistEntries
                .Where(pe => pe.PlaylistId == playlistId)
                .OrderBy(pe => pe.SortOrder)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(pe => new PlaylistEntryEntity
                {
                    Id = pe.Id,
                    PlaylistId = pe.PlaylistId,
                    TrackId = pe.TrackId,
                    SortOrder = pe.SortOrder ?? 0,
                    Guid = pe.Guid
                })
                .ToListAsync();

            // Batch load all tracks in one query
            var trackIds = entries.Select(e => e.TrackId).ToList();
            var tracks = await tracksRepository.GetTracksByIdsAsync(trackIds);
            var trackDict = tracks.ToDictionary(t => t.Id);

            // Assign tracks to entries
            foreach (var entry in entries)
            {
                entry.Track = trackDict.GetValueOrDefault(entry.TrackId)!;
            }

            return new PagedResult<PlaylistEntryEntity>
            {
                Items = entries,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}

     

