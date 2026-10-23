using System.ComponentModel.DataAnnotations;

public class ReorderPlaylistEntriesDto
{
    [Required]
    [MinLength(1)]
    public required List<int> EntryIds { get; set; }
}