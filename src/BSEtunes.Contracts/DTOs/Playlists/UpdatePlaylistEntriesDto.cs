using System.ComponentModel.DataAnnotations;

namespace BSEtunes.Contracts.DTOs.Playlists;

/// <summary>
/// Represents the data transfer object for deleting multiple playlist entries.
/// </summary>
public class UpdatePlaylistEntriesDto
{
    /// <summary>
    /// Gets or sets the collection of playlist entry identifiers to update.
    /// </summary>
    [Required(ErrorMessage = "EntryIds is required.")]
    [MinLength(1, ErrorMessage = "At least one entry ID must be provided.")]
    public required List<int> EntryIds { get; set; }
}