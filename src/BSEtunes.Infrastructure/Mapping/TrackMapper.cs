using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Models;

namespace BSEtunes.Infrastructure.Mapping
{
    public static class TrackMapper
    {
        public static TrackEntity? ToDomain(Track track, Album album, Artist artist)
        {
            if (track == null) return null;
            return new TrackEntity
            {
                Id = track.Id,
                TrackNumber = track.TrackNumber ?? 0,
                Name = track.Name ?? string.Empty,
                Duration = track.Duration?.TimeOfDay ?? TimeSpan.Zero,
                Guid = Guid.TryParse(track.Guid, out var guid) ? guid : Guid.Empty,
                FilePath = track.FilePath ?? string.Empty,
                Extension = string.IsNullOrEmpty(track.FilePath) ? string.Empty : System.IO.Path.GetExtension(track.FilePath),
                Album = new AlbumEntity
                {
                    Id = track.AlbumId,
                    Title = album.Album_Title ?? string.Empty,
                    AlbumId = Guid.TryParse(album.Album_AlbumId, out var albumGuid) ? albumGuid : Guid.Empty,
                    ArtistId = album.Artist_Id,
                    Year = album.Album_Year,
                    GenreId = album.Genre_Id,
                    Genre = new GenreEntity
                    {
                        Id = album.Genre_Id ?? 0,
                        Name = album.Genre_Name ?? string.Empty
                    },
                    Artist = new ArtistEntity
                    {
                        Id = artist.Id,
                        Name = artist.Name ?? string.Empty,
                        SortName = artist.SortName ?? string.Empty
                    }
                }
            };
        }
    }
}
