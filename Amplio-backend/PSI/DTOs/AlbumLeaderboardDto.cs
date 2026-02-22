namespace PSI.DTOs
{
    public class AlbumLeaderboardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public int Popularity { get; set; }
    }
}