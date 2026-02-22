using PSI.Models;

namespace PSI.DTOs
{
    public record SongDto
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public List<Genre> Genres { get; set; } = new List<Genre>();
        public string Link { get; set; } = string.Empty;

        public AlbumDto? Album { get; set; } = null;
    }
}
