namespace PSI.Models
{
    public class Song
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public SongLink Link { get; set; } = new SongLink("https://defaultlink.com");

        public Guid? AlbumId { get; set; }
        public Album? Album { get; set; }
    }
}
