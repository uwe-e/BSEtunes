namespace BSEtunes.Domain.Entities
{
    public class CoverImageEntity
    {
        public byte[] Blob { get; set; } = null!;
        public string Extension { get; set; } = null!;
        public DateTime? LastModified { get; set; }
    }
}
