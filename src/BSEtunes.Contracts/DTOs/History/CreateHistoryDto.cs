namespace BSEtunes.Contracts.DTOs.History
{
    public class CreateHistoryDto
    {
        public int AppId { get; set; }
        public int TitleId { get; set; }
        public int TrackId { get; set; }
        public DateTime? PlayedAt { get; set; }
    }
}