namespace BSEtunes.Contracts.DTOs.Playlists
{
    /// <summary>
    /// Represents a lightweight summary of a playlist for collection endpoints.
    /// </summary>
    public class PlaylistSummaryDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the globally unique identifier (GUID) for the playlist, which can be used for external references and is not tied to the database identity.
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// Gets or sets the name of the playlist.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the owner of the playlist.
        /// </summary>
        public string Owner { get; set; } = string.Empty;
    }
}