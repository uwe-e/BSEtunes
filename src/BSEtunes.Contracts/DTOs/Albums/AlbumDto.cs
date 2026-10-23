namespace BSEtunes.Contracts.DTOs.Albums
{
    /// <summary>
    /// Album Data Transfer Object
    /// </summary>
    public class AlbumDto
    {
        /// <summary>
        /// The Id of the Album
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the album.
        /// </summary>
        public Guid AlbumId { get; set; }
        /// <summary>
        /// Gets or sets the artist associated with this entity.
        /// </summary>
        public ArtistDto Artist { get; set; } = null!;
        /// <summary>
        /// Gets or sets the title of the album.
        /// </summary>
        public string Title { get; set; } = null!;
        /// <summary>
        /// Gets or sets the year of publication of the album.
        /// </summary>
        public int? Year { get; set; }
        /// <summary>
        /// Gets or sets the thumbnail image data associated with the item.
        /// </summary>
        public byte[] Thumbnail { get; set; } = null!;
        /// <summary>
        /// Gets or sets the cover image data associated with the album.
        /// </summary>
        public byte[] Cover { get; set; } = null!;
        /// <summary>
        /// Gets or sets the genre of the album.
        /// </summary>
        public GenreDto Genre { get; set; } = null!;
        /// <summary>
        /// Gets the collection of tracks associated with this album.
        /// </summary>
        public IReadOnlyList<TrackDto> Tracks { get; init; } = new List<TrackDto>();
    }
}
