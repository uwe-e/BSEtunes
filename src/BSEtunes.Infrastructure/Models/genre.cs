using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("genre")]
public partial class Genre
{
    [Column("genreid")]
    public int Id { get; set; }

    [Column("genre1")]
    public string Name { get; set; } = null!;

    [Column("guid")]
    public string Guid { get; set; } = null!;

    [Column("timestamp")]
    public DateTime Timestamp { get; set; }
}
