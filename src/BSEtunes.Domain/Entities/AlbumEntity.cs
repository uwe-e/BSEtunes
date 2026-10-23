namespace BSEtunes.Domain.Entities
{
    public class AlbumEntity
    {
        public int Id { get; set; }
        public Guid AlbumId { get; set; }
        public int ArtistId { get; set; }
        public ArtistEntity Artist { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? Year { get; set; }
        public int ? GenreId { get; set; }
        public GenreEntity Genre { get; set; } = null!;
        public IReadOnlyList<TrackEntity> Tracks { get; init; } = new List<TrackEntity>();

    }
}
