using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSEtunes.Contracts.DTOs.Playlists
{
    /// <summary>
    /// Represents a data transfer object for creating a new playlist,
    /// containing the necessary information such as the name of the playlist.
    /// </summary>
    public class CreatePlaylistDto
    {
        /// <summary>
        /// Gets or sets the name of the playlist to be created.
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
