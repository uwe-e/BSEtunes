namespace BSEtunes.Domain.Entities
{
    public class TrackEntity
    {
        public int Id { get; set; }
        public int TrackNumber { get; set; }
        public string Name { get; set; } = null!;
        public TimeSpan Duration { get; set; }
        public Guid Guid { get; set; }
        public string FilePath { get; set; } = null!;
        public string Extension { get; set; } = null!;
        public AlbumEntity? Album { get; set; }
    }
}
