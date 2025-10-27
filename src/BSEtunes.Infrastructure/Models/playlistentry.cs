using System;
using System.Collections.Generic;

namespace BSEtunes.Infrastructure.Models;

public partial class playlistentry
{
    public int EntryId { get; set; }

    public int PlaylistId { get; set; }

    public int LiedId { get; set; }

    public int? sortorder { get; set; }

    public Guid Guid { get; set; }

    public DateTime Timestamp { get; set; }
}
