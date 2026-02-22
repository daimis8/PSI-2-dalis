namespace PSI.DTOs
{
    public class PlaylistLeaderboardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Popularity { get; set; }
        public bool IsPublic { get; set; }
    }
}