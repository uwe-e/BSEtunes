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
        public async Task AppendPlaylistEntriesAsync(int playlistId, List<int> trackIds)
        {
            var stopwatch = Stopwatch.StartNew();

            // Get the current max sort order for the playlist
            var maxSortOrder = await context.PlaylistEntries
                .Where(pe => pe.PlaylistId == playlistId)
                .MaxAsync(pe => (int?)pe.SortOrder) ?? -1;

            // Create new playlist entries with incrementing sort order
            var newEntries = trackIds.Select((trackId, index) => new PlaylistEntry
            {
                PlaylistId = playlistId,
                TrackId = trackId,
                SortOrder = maxSortOrder + index + 1,
                Guid = Guid.NewGuid()
            }).ToList();

            // Add entries and save
            context.PlaylistEntries.AddRange(newEntries);
            await context.SaveChangesAsync();

            stopwatch.Stop();
            logger.LogDebug("AppendPlaylistEntriesAsync took {ElapsedMs}ms for playlistId {PlaylistId}, added {Count} entries",
                stopwatch.ElapsedMilliseconds, playlistId, trackIds.Count);
        }

        public async Task<PlaylistSummaryEntity> CreatePlaylistAsync(PlaylistEntity playlist)
        {
            var stopwatch = Stopwatch.StartNew();

            // Map PlaylistEntity to Playlist model
            var playlistModel = new Playlist
            {
                Name = playlist.Name,
                Owner = playlist.Owner,
                Guid = (playlist.Guid == Guid.Empty ? Guid.NewGuid() : playlist.Guid).ToString()
            };

            // Add to context and save
            context.Playlists.Add(playlistModel);
            await context.SaveChangesAsync();

            stopwatch.Stop();
            logger.LogDebug("CreatePlaylistAsync took {ElapsedMs}ms for owner {Owner}, created playlist {PlaylistId}",
                stopwatch.ElapsedMilliseconds, playlistModel.Owner, playlistModel.Id);

            // Return summary entity
            return new PlaylistSummaryEntity
            {
                Id = playlistModel.Id,
                Name = playlistModel.Name,
                Owner = playlistModel.Owner,
                Guid = playlistModel.Guid,
                EntryCount = 0,
                CoverAlbumIds = []
            };
        }

        public async Task<bool> DeletePlaylistAsync(int playlistId, string owner)
        {
            var stopwatch = Stopwatch.StartNew();

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var playlist = await context.Playlists
                    .FirstOrDefaultAsync(p => p.Id == playlistId && p.Owner == owner);

                if (playlist == null)
                {
                    await transaction.RollbackAsync(); // Explicit rollback
                    logger.LogDebug("DeletePlaylistAsync: Playlist {PlaylistId} not found", playlistId);
                    return false;
                }

                await context.PlaylistEntries
                    .Where(pe => pe.PlaylistId == playlistId)
                    .ExecuteDeleteAsync(); // Bulk delete (EF Core 7+)

                context.Playlists.Remove(playlist);
                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                stopwatch.Stop();
                logger.LogDebug("DeletePlaylistAsync took {ElapsedMs}ms for playlistId {PlaylistId}",
                    stopwatch.ElapsedMilliseconds, playlistId);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting playlist {PlaylistId}", playlistId);
                throw;
            }
        }

        public async Task<int> DeletePlaylistEntriesAsync(int playlistId, List<int> entryIds, string owner)
        {
            var stopwatch = Stopwatch.StartNew();

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                // Verify playlist ownership
                var playlistExists = await context.Playlists
                    .AnyAsync(p => p.Id == playlistId && p.Owner == owner);

                if (!playlistExists)
                {
                    await transaction.RollbackAsync();
                    logger.LogDebug("DeletePlaylistEntriesAsync: Playlist {PlaylistId} not found or not owned by {Owner}",
                        playlistId, owner);
                    return 0;
                }

                // Get entries to delete with their sort orders
                var entriesToDelete = await context.PlaylistEntries
                    .Where(pe => pe.PlaylistId == playlistId && entryIds.Contains(pe.Id))
                    .Select(pe => new { pe.Id, pe.SortOrder })
                    .ToListAsync();

                if (entriesToDelete.Count == 0)
                {
                    await transaction.RollbackAsync();
                    logger.LogDebug("DeletePlaylistEntriesAsync: No entries found to delete in playlist {PlaylistId}",
                        playlistId);
                    return 0;
                }

                var minSortOrder = entriesToDelete.Min(e => e.SortOrder);
                var deletedCount = entriesToDelete.Count;

                // Bulk delete the entries
                await context.PlaylistEntries
                    .Where(pe => pe.PlaylistId == playlistId && entryIds.Contains(pe.Id))
                    .ExecuteDeleteAsync();

                // Reorder remaining entries after the minimum deleted sort order
                await context.PlaylistEntries
                    .Where(pe => pe.PlaylistId == playlistId && pe.SortOrder > minSortOrder)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(
                        pe => pe.SortOrder,
                        pe => pe.SortOrder - deletedCount));

                await transaction.CommitAsync();

                stopwatch.Stop();
                logger.LogDebug("DeletePlaylistEntriesAsync took {ElapsedMs}ms for {Count} entries in playlistId {PlaylistId}",
                    stopwatch.ElapsedMilliseconds, deletedCount, playlistId);

                return deletedCount;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error deleting playlist entries from playlist {PlaylistId}",
                    playlistId);
                throw;
            }
        }

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
        
        public async Task<IReadOnlyList<PlaylistEntity>> GetPlaylistsByOwnerAsync(string owner)
        {
            var stopwatch = Stopwatch.StartNew();

            var playlists = await context.Playlists
                .Where(p => p.Owner == owner)
                .OrderBy(p => p.Name)
                .Select(p => new PlaylistEntity
                {
                    Id = p.Id,
                    Name = p.Name,
                    Owner = p.Owner,
                    Guid = Guid.Parse(p.Guid)
                })
                .ToListAsync(); // Returns List<T> which implements IReadOnlyList<T>

            stopwatch.Stop();
            logger.LogDebug("GetPlaylistsByOwnerAsync took {ElapsedMs}ms for owner {Owner}, returned {Count} playlists",
                stopwatch.ElapsedMilliseconds, owner, playlists.Count);

            return playlists;
        }
        /// <summary>
        /// Retrieves all track IDs for a specific playlist ordered by sort order.
        /// </summary>
        /// <param name="playlistId">The unique identifier of the playlist.</param>
        /// <returns>A list of track IDs ordered by their sort order in the playlist.</returns>
        public async Task<List<int>> GetTrackIdsByPlaylistIdAsync(int playlistId, bool randomize = false)
        {
            var stopwatch = Stopwatch.StartNew();

            var query = context.PlaylistEntries
                .Where(pe => pe.PlaylistId == playlistId);

            // Apply ordering based on randomize flag
            var orderedQuery = randomize
                ? query.OrderBy(pe => EF.Functions.Random()) // Database-level randomization
                : query.OrderBy(pe => pe.SortOrder);

            var trackIds = await orderedQuery
                .Select(pe => pe.TrackId)
                .ToListAsync();

            stopwatch.Stop();
            logger.LogDebug("GetTrackIdsByPlaylistIdAsync took {ElapsedMs}ms for playlistId {PlaylistId}, returned {Count} track IDs (randomized: {Randomized})",
                stopwatch.ElapsedMilliseconds, playlistId, trackIds.Count, randomize);

            return trackIds;
        }
        
        
    }
}

