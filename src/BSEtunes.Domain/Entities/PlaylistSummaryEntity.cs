namespace BSEtunes.Domain.Entities
{
    public class PlaylistSummaryEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public string Guid { get; set; } = null!;
        public long EntryCount { get; set; }
        public IReadOnlyList<string> CoverAlbumIds { get; set; } = [];
    }
}
