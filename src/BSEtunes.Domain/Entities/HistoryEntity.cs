namespace BSEtunes.Domain.Entities
{
    public class HistoryEntity
    {
        public int Id { get; set; }
        public int AppId { get; set; }
        public int TitleId { get; set; }
        public int TrackId { get; set; }
        public DateTime PlayedAt { get; set; }
        public string Artist { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string TrackName { get; set; } = null!;
        public string Owner { get; set; } = null!;
    }
}