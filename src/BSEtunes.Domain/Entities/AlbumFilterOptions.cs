namespace BSEtunes.Domain.Entities
{
    public class AlbumFilterOptions
    {
        public string? Genre { get; set; }
        public string? ArtistName { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
    }
}
