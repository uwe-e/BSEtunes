using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class playlistswithnumberofentry
{
    public int ListId { get; set; }

    public string ListName { get; set; } = null!;

    public string User { get; set; } = null!;

    public string guid { get; set; } = null!;

    public long Number { get; set; }
}
