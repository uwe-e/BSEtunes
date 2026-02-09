using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Mapping;
using BSEtunes.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Data;

namespace BSEtunes.Infrastructure.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly RecordsDbContext _context;

        public SearchRepository(RecordsDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<AlbumEntity>> GetAlbumSearchAsync(
            string searchPhrase, int pageSize, int pageIndex)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "GetAlbumSearchWithCount";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("@searchPhrase", searchPhrase ?? string.Empty));
            command.Parameters.Add(new MySqlParameter("@pageSize", pageSize));
            command.Parameters.Add(new MySqlParameter("@pageIndex", (pageIndex - 1) * pageSize)); // Convert pageIndex to offset

            var totalCountParam = new MySqlParameter("@totalCount", MySqlDbType.Int32)
            {
                Direction = System.Data.ParameterDirection.Output
            };
            command.Parameters.Add(totalCountParam);

            var albums = new List<Models.Album>();
            
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    albums.Add(MapAlbumFromReader(reader));
                }
            }

            int totalCount = totalCountParam.Value != DBNull.Value 
                ? Convert.ToInt32(totalCountParam.Value) 
                : 0;

            var domainAlbums = AlbumMapper.ToDomain(albums) ?? Enumerable.Empty<AlbumEntity>();

            return new PagedResult<AlbumEntity>
            {
                Items = domainAlbums,
                TotalCount = totalCount,
                PageNumber = pageIndex, // Convert from 0-based to 1-based
                PageSize = pageSize
            };
        }

        public async Task<PagedResult<TrackEntity>> GetTrackSearchAsync(
            string searchPhrase, int pageSize, int pageIndex)
        {
            var connection = _context.Database.GetDbConnection();
            await connection.OpenAsync();

            using var command = connection.CreateCommand();
            command.CommandText = "GetTrackSearchWithCount";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new MySqlParameter("@searchPhrase", searchPhrase ?? string.Empty));
            command.Parameters.Add(new MySqlParameter("@pageSize", pageSize));
            command.Parameters.Add(new MySqlParameter("@pageIndex", (pageIndex - 1) * pageSize));

            var totalCountParam = new MySqlParameter("@totalCount", MySqlDbType.Int32)
            {
                Direction = ParameterDirection.Output
            };
            command.Parameters.Add(totalCountParam);

            var tracks = new List<TrackEntity>(pageSize);

            using (var reader = await command.ExecuteReaderAsync())
            {
                // Cache ordinal lookups outside the loop for better performance
                var trackIdOrdinal = reader.GetOrdinal("TrackId");
                var trackNameOrdinal = reader.GetOrdinal("Track");
                var durationOrdinal = reader.GetOrdinal("Duration");
                var albumIdOrdinal = reader.GetOrdinal("AlbumId");
                var albumNameOrdinal = reader.GetOrdinal("AlbumName");
                var albumGuidOrdinal = reader.GetOrdinal("Guid");
                var artistIdOrdinal = reader.GetOrdinal("ArtistId");
                var artistNameOrdinal = reader.GetOrdinal("ArtistName");

                while (await reader.ReadAsync())
                {
                    // Read duration as TimeSpan from MySQL TIME column
                    var duration = reader.IsDBNull(durationOrdinal)
                        ? TimeSpan.Zero : (TimeSpan)reader.GetValue(durationOrdinal);

                    // Create all three objects inline and map directly
                    var track = new Models.Track
                    {
                        Id = reader.GetInt32(trackIdOrdinal),
                        Name = reader.GetString(trackNameOrdinal),
                        Duration = DateTime.Today.Add(duration),
                        AlbumId = reader.GetInt32(albumIdOrdinal),
                        Guid = reader.GetString(albumGuidOrdinal),
                        Timestamp = DateTime.UtcNow,
                        FilePath = null,
                        TrackNumber = null
                    };

                    var album = new Models.Album
                    {
                        Album_Id = reader.GetInt32(albumIdOrdinal),
                        Artist_Id = reader.GetInt32(artistIdOrdinal),
                        Artist_Name = reader.GetString(artistNameOrdinal),
                        Artist_SortName = reader.GetString(artistNameOrdinal),
                        Album_Title = reader.GetString(albumNameOrdinal),
                        Album_AlbumId = reader.GetString(albumGuidOrdinal),
                        Album_Year = null,
                        Genre_Id = null,
                        Genre_Name = null
                    };

                    var artist = new Models.Artist
                    {
                        Id = reader.GetInt32(artistIdOrdinal),
                        Name = reader.GetString(artistNameOrdinal),
                        SortName = reader.GetString(artistNameOrdinal),
                        Guid = string.Empty,
                        Timestamp = DateTime.UtcNow
                    };

                    var trackEntity = TrackMapper.ToDomain(track, album, artist);
                    if (trackEntity != null)
                    {
                        tracks.Add(trackEntity);
                    }
                }
            }

            int totalCount = totalCountParam.Value != DBNull.Value
                ? Convert.ToInt32(totalCountParam.Value)
                : 0;

            return new PagedResult<TrackEntity>
            {
                Items = tracks,
                TotalCount = totalCount,
                PageNumber = pageIndex,
                PageSize = pageSize
            };
        }

        private Album MapAlbumFromReader(System.Data.Common.DbDataReader reader)
        {
            return new Album
            {
                Album_Id = reader.GetInt32(reader.GetOrdinal("AlbumId")),
                Artist_Id = reader.GetInt32(reader.GetOrdinal("ArtistId")),
                Artist_Name = reader.GetString(reader.GetOrdinal("ArtistName")),
                Artist_SortName = reader.GetString(reader.GetOrdinal("ArtistName")), // Using ArtistName as SortName since not returned
                Album_Title = reader.GetString(reader.GetOrdinal("AlbumName")),
                Album_AlbumId = reader.GetString(reader.GetOrdinal("Guid")),
                Album_Year = null, // Not returned by stored procedure
                Genre_Id = null, // Not returned by stored procedure
                Genre_Name = null // Not returned by stored procedure
            };
        }

        //private TrackSearchResult MapTrackFromReader(System.Data.Common.DbDataReader reader)
        //{
        //    return new TrackSearchResult
        //    {
        //        AlbumId = reader.GetInt32(reader.GetOrdinal("AlbumId")),
        //        ArtistId = reader.GetInt32(reader.GetOrdinal("ArtistId")),
        //        ArtistName = reader.GetString(reader.GetOrdinal("ArtistName")),
        //        AlbumName = reader.GetString(reader.GetOrdinal("AlbumName")),
        //        AlbumGuid = reader.GetString(reader.GetOrdinal("Guid")),
        //        TrackId = reader.GetInt32(reader.GetOrdinal("TrackId")),
        //        Track = reader.GetString(reader.GetOrdinal("Track")),
        //        Duration = reader.GetTimeSpan(reader.GetOrdinal("Duration"))
        //    };
        //}
    }
}
