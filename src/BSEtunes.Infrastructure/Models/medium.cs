using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class medium
{
    public int MediumID { get; set; }

    public string Medium { get; set; } = null!;

    public string? Beschreibung { get; set; }

    public string Guid { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
