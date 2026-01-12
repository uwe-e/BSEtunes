using BSEtunes.Contracts.DTOs.Albums;

namespace BSEtunes.Contracts.DTOs.Playlists
{
    /// <summary>
    /// Represents a data transfer object for an entry in a playlist, including references to the associated playlist
    /// and track.
    /// </summary>
    /// <remarks>This class is typically used to transfer playlist entry data between application layers or
    /// over service boundaries. It contains identifiers and navigation properties for both the playlist and the track.
    /// The navigation properties may be null if not populated by the data source.</remarks>
    public class PlaylistEntryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the playlist.
        /// </summary>
        public int PlaylistId { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for the track.
        /// </summary>
        public int TrackId { get; set; }
        /// <summary>
        /// Gets or sets the optional sort order for the item.
        /// </summary>
        /// <remarks>A lower value indicates a higher priority in sorting. If <see langword="null"/>, the
        /// item does not have an explicit sort order and may be ordered according to default logic.</remarks>
        public int? SortOrder { get; set; }
        /// <summary>
        /// Gets or sets the unique identifier for this instance.
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// Gets or sets the playlist associated with the current context.
        /// </summary>
        public PlaylistDto? Playlist { get; set; } = null;
        /// <summary>
        /// Gets or sets the track information associated with this instance.
        /// </summary>
        public TrackDto? Track { get; set; } = null;

    }
}
