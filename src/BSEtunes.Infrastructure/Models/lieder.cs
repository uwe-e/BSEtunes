using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class lieder
{
    public int LiedID { get; set; }

    public int TitelID { get; set; }

    public int? Track { get; set; }

    public string? Lied { get; set; }

    public DateTime? Dauer { get; set; }

    public string? Liedpfad { get; set; }

    public string guid { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
