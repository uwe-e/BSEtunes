namespace BSEtunes.Contracts.DTOs.Playlists
{
    /// <summary>
    /// Represents a data transfer object for a playlist, containing identifying information, ownership details, and a
    /// collection of playlist entries.
    /// </summary>
    /// <remarks>This class is typically used to transfer playlist data between application layers or over
    /// service boundaries. It includes both metadata about the playlist and the list of entries it contains. The
    /// properties are designed for serialization and may be used in API responses or requests.</remarks>
    public class PlaylistDto
    {
        /// <summary>
        /// Gets or sets the unique identifier of the playlist.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the playlist.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// Gets or sets the owner of the playlist.
        /// </summary>
        public string Owner { get; set; } = null!;
        /// <summary>
        /// Gets or sets the globally unique identifier of the playlist.
        /// </summary>
        public string Guid { get; set; } = null!;
        /// <summary>
        /// Gets or sets the total number of entries in the playlist.
        /// </summary>
        public int? EntryCount { get; set; }
        /// <summary>
        /// Gets the list of album IDs that are used as cover images.
        /// </summary>
        public IReadOnlyList<string> CoverAlbumIds { get; init; } = new List<string>();
        /// <summary>
        /// Gets the list of entries in the playlist.
        /// </summary>
        public IReadOnlyList<PlaylistEntryDto> Entries { get; init; } = new List<PlaylistEntryDto>();
    }
}
