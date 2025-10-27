using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class genre
{
    public int genreid { get; set; }

    public string genre1 { get; set; } = null!;

    public string guid { get; set; } = null!;

    public DateTime timestamp { get; set; }
}
