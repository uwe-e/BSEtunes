namespace BSEtunes.Contracts.DTOs.Albums
{
    /// <summary>
    /// Represents a data transfer object (DTO) for an artist, containing identifying and display information.
    /// </summary>
    /// <remarks>Use this type to transfer artist data between application layers or services without exposing
    /// domain or persistence details. The properties provide basic information suitable for display, sorting, or
    /// identification purposes.</remarks>
    public class ArtistDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the artist.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the artist.
        /// </summary>
        public string Name { get; set; } = null!;
        /// <summary>
        /// Gets or sets the sort name of the artist.
        /// </summary>
        public string SortName { get; set; } = null!;
    }
}
