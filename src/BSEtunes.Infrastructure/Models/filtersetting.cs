using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class filtersetting
{
    public int filterid { get; set; }

    public int mode { get; set; }

    public string? value { get; set; }

    public ulong isused { get; set; }

    public string benutzer { get; set; } = null!;
}
