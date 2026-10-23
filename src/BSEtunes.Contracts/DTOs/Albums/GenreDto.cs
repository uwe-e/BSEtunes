namespace BSEtunes.Contracts.DTOs.Albums
{
    /// <summary>
    /// Represents a data transfer object for a genre, containing its unique identifier and name.
    /// </summary>
    public class GenreDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the genre.
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the name of the genre.
        /// </summary>
        public string Name { get; set; } = null!;
    }
}
