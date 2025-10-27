using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class interpreten
{
    public int InterpretID { get; set; }

    public string Interpret { get; set; } = null!;

    public string Interpret_Lang { get; set; } = null!;

    public string Guid { get; set; } = null!;

    public DateTime Timestamp { get; set; }
}
