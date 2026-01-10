namespace BSEtunes.Application.DTOs
{
    public class PlaylistEntryDto
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int TrackId { get; set; }
        public int? SortOrder { get; set; }
        public Guid Guid { get; set; }
        public PlaylistDto? Playlist { get; set; } = null;

    }
}
