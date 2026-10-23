using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("playlistswithnumberofentry")]
public partial class PlaylistSummary
{
    [Column("ListId")]
    public int Id { get; set; }
    [Column("ListName")]
    public string Name { get; set; } = null!;
    [Column("User")]
    public string Owner { get; set; } = null!;
    [Column("guid")]
    public string Guid { get; set; } = null!;
    [Column("Number")]
    public long EntryCount { get; set; }
}
