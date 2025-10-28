namespace BSEtunes.Application.DTOs
{
    public class TrackDto
    {
        public int Id { get; set; }
        public int TrackNumber { get; set; }
        public string Name { get; set; } = null!;
        public TimeSpan Duration { get; set; }
        public Guid Guid { get; set; }
        public string Extension { get; set; } = null!;
    }
}
