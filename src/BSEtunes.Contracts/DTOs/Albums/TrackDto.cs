namespace BSEtunes.Contracts.DTOs.Albums
{
    /// <summary>
    /// Represents a data transfer object (DTO) for a music track, containing identifying and descriptive information
    /// about the track and its associated album.
    /// </summary>
    /// <remarks>Use this type to transfer track data between application layers or services without exposing
    /// domain or persistence details. The properties provide essential metadata for identifying, displaying, and
    /// managing tracks within an album context. This DTO is typically used in scenarios such as API responses, view
    /// models, or service contracts.</remarks>
    public class TrackDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the track.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the track number within the album.
        /// </summary>
        public int TrackNumber { get; set; }
        /// <summary>
        /// Gets or sets the name of the track.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// Gets or sets the duration of the track.
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Gets or sets the globally unique identifier of the track.
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// Gets or sets the file extension of the track.
        /// </summary>
        public string Extension { get; set; } = null!;
        /// <summary>
        /// Gets or sets the album to which the track belongs.
        /// </summary>
        public AlbumDto Album { get; set; } = null!;
    }
}
