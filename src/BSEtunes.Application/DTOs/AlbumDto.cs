using BSEtunes.Domain.Entities;

namespace BSEtunes.Application.DTOs
{
    public class AlbumDto
    {
        public int Id { get; set; }
        public Guid AlbumId { get; set; }
        public ArtistDto Artist { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? Year { get; set; }
        public byte[] Thumbnail { get; set; } = null!;
        public byte[] Cover { get; set; } = null!;
        public GenreDto Genre { get; set; } = null!;
        public IReadOnlyList<TrackDto> Tracks { get; init; } = new List<TrackDto>();
    }
}
