namespace BSEtunes.Domain.Entities
{
    public class PlaylistEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Owner { get; set; } = null!;
        public Guid Guid { get; set; }
    }
}
