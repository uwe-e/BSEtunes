using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("playlistentries")]
public partial class PlaylistEntry
{
    [Column("EntryId")]
    public int Id { get; set; }
    
    [Column("PlaylistId")]
    public int PlaylistId { get; set; }
    [Column("LiedId")]
    public int TrackId { get; set; }
    [Column("sortorder")]
    public int? SortOrder { get; set; }
    [Column("Guid")]
    public Guid Guid { get; set; }

    public DateTime Timestamp { get; set; }
}
