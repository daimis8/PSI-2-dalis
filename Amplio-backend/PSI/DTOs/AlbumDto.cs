namespace PSI.DTOs
{
    public record AlbumDto
    {
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
    }
}
