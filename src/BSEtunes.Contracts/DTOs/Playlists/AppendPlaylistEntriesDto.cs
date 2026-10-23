using System.ComponentModel.DataAnnotations;

namespace BSEtunes.Contracts.DTOs.Playlists
{
    /// <summary>
    /// Represents the data transfer object for appending track entries to a playlist.
    /// </summary>
    public class AppendPlaylistEntriesDto
    {
        /// <summary>
        /// Gets or sets the collection of track identifiers to append to the playlist.
        /// </summary>
        [Required(ErrorMessage = "TrackIds is required.")]
        [MinLength(1, ErrorMessage = "At least one track ID must be provided.")]
        public required List<int> TrackIds { get; set; }
    }
}