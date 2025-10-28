namespace BSEtunes.Domain.Entities
{
    public class AlbumEntity
    {
        public int Id { get; set; }
        public Guid AlbumId { get; set; }
        public ArtistEntity Artist { get; set; } = null!;
        public string Title { get; set; } = null!;
        public int? Year { get; set; }
        public byte[] Thumbnail { get; set; } = null!;
        public byte[] Cover { get; set; } = null!;
        public GenreEntity Genre { get; set; } = null!;
        public IReadOnlyList<TrackEntity> Tracks { get; init; } = new List<TrackEntity>();

    }
}
