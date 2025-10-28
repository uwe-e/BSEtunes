using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BSEtunes.Infrastructure.Models;

[Table("album")]
public partial class Album
{
    public int Artist_Id { get; set; }

    public string Artist_Name { get; set; } = null!;

    public string Artist_SortName { get; set; } = null!;

    public int Album_Id { get; set; }

    public string Album_Title { get; set; } = null!;

    public string Album_AlbumId { get; set; } = null!;

    public int? Album_Year { get; set; }

    public int? Genre_Id { get; set; }

    public string? Genre_Name { get; set; }
}
