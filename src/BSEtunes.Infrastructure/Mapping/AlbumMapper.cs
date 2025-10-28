using BSEtunes.Domain.Entities;
using BSEtunes.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSEtunes.Infrastructure.Mapping
{
    public static class AlbumMapper
    {
        public static AlbumEntity ToDomain(Album model, IEnumerable<Track> dbTracks)
        {
            if (model == null) return null;
            return new AlbumEntity
            {
                Id = model.Album_Id,
                AlbumId = Guid.TryParse(model.Album_AlbumId, out var guid) ? guid : Guid.Empty,
                Title = model.Album_Title,
                Artist = new ArtistEntity
                {
                    Id = model.Artist_Id,
                    Name = model.Artist_Name,
                    SortName = model.Artist_SortName
                },
                Year = model.Album_Year,
                Tracks = dbTracks.Select(t => new TrackEntity
                {
                    Id = t.Id,
                    TrackNumber = t.TrackNumber ?? 0,
                    Name = t.Name ?? string.Empty,
                    Duration = t.Duration?.TimeOfDay ?? TimeSpan.Zero,
                    Guid = Guid.TryParse(t.Guid, out var guid) ? guid : Guid.Empty,
                    Extension = string.IsNullOrEmpty(t.FilePath) ? string.Empty : System.IO.Path.GetExtension(t.FilePath)
                }).ToList(),
                Genre = new GenreEntity
                { 
                    Id = model.Genre_Id ?? 0,
                    Name = model.Genre_Name ?? string.Empty
                }
            };
        }
    }
}
