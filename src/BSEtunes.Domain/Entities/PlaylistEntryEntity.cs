namespace BSEtunes.Domain.Entities
{
    public class PlaylistEntryEntity
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int TrackId { get; set; }
        public int SortOrder { get; set; }
        public Guid Guid { get; set; }
        public TrackEntity Track { get; set; } = null!;
    }
}
