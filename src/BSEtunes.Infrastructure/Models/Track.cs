using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("lieder")]
public partial class Track
{
    [Column("LiedID")]
    public int Id { get; set; }
    [Column("TitelID")]
    public int AlbumId { get; set; }
    [Column("Track")]
    public int? TrackNumber { get; set; }
    [Column("Lied")]
    public string? Name { get; set; }
    [Column("Dauer")]
    public DateTime? Duration { get; set; }
    [Column("Liedpfad")]
    public string? FilePath { get; set; }
    [Column("guid")]
    public string Guid { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
