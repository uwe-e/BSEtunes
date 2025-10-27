using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class titel
{
    public int TitelID { get; set; }

    public int InterpretID { get; set; }

    public string Titel { get; set; } = null!;

    public int? ErschDatum { get; set; }

    public int? MediumID { get; set; }

    public int? mp3tag { get; set; }

    public string Guid { get; set; } = null!;

    public string? PictureFormat { get; set; }

    public byte[]? Cover { get; set; }

    public byte[]? thumbnail { get; set; }

    public DateTime? ErstellDatum { get; set; }

    public string? ErstellerNm { get; set; }

    public DateTime? MutationDatum { get; set; }

    public string? MutationNm { get; set; }

    public DateTime Timestamp { get; set; }

    public int? genreId { get; set; }
}
