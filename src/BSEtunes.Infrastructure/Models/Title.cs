using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("titel")]
public partial class Title
{
    [Column("TitelID")]
    public int Id { get; set; }

    public int InterpretID { get; set; }

    public string Titel { get; set; } = null!;

    public int? ErschDatum { get; set; }

    public int? MediumID { get; set; }
    [Column("mp3tag")]
    public int? Mp3tag { get; set; }

    public string Guid { get; set; } = null!;
    
    [Column("PictureFormat")]
    public string? Extension { get; set; }

    public byte[]? Cover { get; set; }
    
    [Column("thumbnail")]
    public byte[]? Thumbnail { get; set; }

    public DateTime? ErstellDatum { get; set; }

    public string? ErstellerNm { get; set; }
    
    [Column("MutationDatum")]
    public DateTime? MutationDate { get; set; }
    
    public string? MutationNm { get; set; }

    public DateTime Timestamp { get; set; }
    [Column("genreId")]
    public int? GenreId { get; set; }
}
