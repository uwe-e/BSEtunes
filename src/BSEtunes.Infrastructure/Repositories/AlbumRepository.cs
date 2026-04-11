using BSEtunes.Contracts.Enums;
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

        public async Task<IEnumerable<AlbumEntity>> GetSortedAlbumsAsync(AlbumSortOption sortBy = AlbumSortOption.Random, int limit = 10)
        {
            IQueryable<Models.Album> query = _context.Albums;

            query = sortBy switch
            {
                AlbumSortOption.Title => query.OrderBy(a => a.Album_Title),
                AlbumSortOption.TitleDesc => query.OrderByDescending(a => a.Album_Title),
                AlbumSortOption.Artist => query.OrderBy(a => a.Artist_Name),
                AlbumSortOption.ArtistDesc => query.OrderByDescending(a => a.Artist_Name),
                AlbumSortOption.Year => query.OrderBy(a => a.Album_Year),
                AlbumSortOption.YearDesc => query.OrderByDescending(a => a.Album_Year),
                AlbumSortOption.Newest => query.OrderBy(a => a.Album_Id),
                AlbumSortOption.NewestDesc => query.OrderByDescending(a => a.Album_Id),
                _ => query.OrderByDescending(a => EF.Functions.Random())
            };

            var albums = await query
                .Take(limit)
                .ToListAsync();

            return AlbumMapper.ToDomain(albums) ?? Enumerable.Empty<AlbumEntity>();
        }

        public async Task<PagedResult<AlbumEntity>> GetPagedAlbumsAsync(
            AlbumFilterOptions? filterOptions = null,
            AlbumSortOption sortBy = AlbumSortOption.Random,
            int pageNumber = 1,
            int pageSize = 10)
        {
            IQueryable<Models.Album> query = _context.Albums;

            // Apply filters
            if (filterOptions != null)
            {
                if (!string.IsNullOrWhiteSpace(filterOptions.Genre))
                {
                    query = query.Where(a => a.Genre_Name != null && a.Genre_Name.Contains(filterOptions.Genre));
                }
                
                if (filterOptions.ArtistId.HasValue)
                {
                    query = query.Where(a => a.Artist_Id == filterOptions.ArtistId.Value);
                }

                if (!string.IsNullOrWhiteSpace(filterOptions.ArtistName))
                {
                    query = query.Where(a => a.Artist_Name != null && a.Artist_Name.Contains(filterOptions.ArtistName));
                }

                if (filterOptions.YearFrom.HasValue)
                {
                    query = query.Where(a => a.Album_Year >= filterOptions.YearFrom.Value);
                }

                if (filterOptions.YearTo.HasValue)
                {
                    query = query.Where(a => a.Album_Year <= filterOptions.YearTo.Value);
                }
            }

            // Get total count before pagination
            var totalCount = await query.CountAsync();

            // Apply sorting
            query = sortBy switch
            {
                AlbumSortOption.Title => query.OrderBy(a => a.Album_Title),
                AlbumSortOption.TitleDesc => query.OrderByDescending(a => a.Album_Title),
                AlbumSortOption.Artist => query.OrderBy(a => a.Artist_Name),
                AlbumSortOption.ArtistDesc => query.OrderByDescending(a => a.Artist_Name),
                AlbumSortOption.Year => query.OrderBy(a => a.Album_Year),
                AlbumSortOption.YearDesc => query.OrderByDescending(a => a.Album_Year),
                AlbumSortOption.Newest => query.OrderBy(a => a.Album_Id),
                AlbumSortOption.NewestDesc => query.OrderByDescending(a => a.Album_Id),
                _ => query.OrderByDescending(a => EF.Functions.Random())
            };

            // Apply pagination
            var albums = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<AlbumEntity>
            {
                Items = AlbumMapper.ToDomain(albums) ?? Enumerable.Empty<AlbumEntity>(),
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<TrackEntity>> GetPagedTracksByAlbumIdAsync(
            int albumId,
            int pageNumber = 1,
            int pageSize = 20)
        {
            // First, verify the album exists
            var albumExists = await _context.Albums
                .AnyAsync(a => a.Album_Id == albumId);

            if (!albumExists)
            {
                return new PagedResult<TrackEntity>
                {
                    Items = Enumerable.Empty<TrackEntity>(),
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            // Get total count of tracks for this album
            var totalCount = await _context.Tracks
                .Where(t => t.AlbumId == albumId)
                .CountAsync();

            // Get paginated tracks
            var tracks = await _context.Tracks
                .Where(t => t.AlbumId == albumId)
                .OrderBy(t => t.TrackNumber)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Map to domain entities
            var trackEntities = tracks.Select(t => new TrackEntity
            {
                Id = t.Id,
                TrackNumber = t.TrackNumber ?? 0,
                Name = t.Name ?? string.Empty,
                Duration = t.Duration.HasValue
                    ? TimeSpan.FromSeconds((t.Duration.Value - DateTime.MinValue).TotalSeconds)
                    : TimeSpan.Zero,
                Guid = Guid.Parse(t.Guid),
                FilePath = t.FilePath ?? string.Empty,
                Extension = Path.GetExtension(t.FilePath ?? string.Empty)
            });

            return new PagedResult<TrackEntity>
            {
                Items = trackEntities,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
