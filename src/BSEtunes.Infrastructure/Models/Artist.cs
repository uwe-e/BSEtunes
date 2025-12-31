using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("interpreten")]
public partial class Artist
{
    [Column("InterpretID")]
    public int Id { get; set; }

    [Column("Interpret")]
    public string Name { get; set; } = null!;
    [Column("Interpret_Lang")]
    public string SortName { get; set; } = null!;

    public string Guid { get; set; } = null!;

    public DateTime Timestamp { get; set; }

}
