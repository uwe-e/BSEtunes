using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class history
{
    public int PlayID { get; set; }

    public int AppID { get; set; }

    public int TitelID { get; set; }

    public int LiedID { get; set; }

    public DateTime Zeit { get; set; }

    public string Interpret { get; set; } = null!;

    public string Titel { get; set; } = null!;

    public string Lied { get; set; } = null!;

    public string Benutzer { get; set; } = null!;
}
