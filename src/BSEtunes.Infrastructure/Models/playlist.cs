using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class playlist
{
    public int ListId { get; set; }

    public string ListName { get; set; } = null!;

    public string User { get; set; } = null!;

    public string guid { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
