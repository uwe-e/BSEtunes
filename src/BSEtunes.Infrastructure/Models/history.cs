using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("history")]
public partial class History
{
    [Column("PlayID")]
    public int Id { get; set; }
    public int AppID { get; set; }
    [Column("TitelID")]
    public int TitleId { get; set; }
    [Column("LiedID")]
    public int TrackId { get; set; }
    [Column("Zeit")]
    public DateTime PlayedAt { get; set; }
    [Column("Interpret")]
    public string Artist { get; set; } = null!;
    [Column("Titel")]
    public string Title { get; set; } = null!;
    [Column("Lied")]
    public string TrackName { get; set; } = null!;
    [Column("Benutzer")]
    public string Owner { get; set; } = null!;
}
