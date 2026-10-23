using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("playlist")]
public partial class Playlist
{
    [Column("ListId")]
    public int Id { get; set; }
    [Column("ListName")]
    public string Name { get; set; } = null!;
    [Column("User")]
    public string Owner { get; set; } = null!;
    [Column("Guid")]
    public string Guid { get; set; } = null!;
    public DateTime Timestamp { get; set; }
}
