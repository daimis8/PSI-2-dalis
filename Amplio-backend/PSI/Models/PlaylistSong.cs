namespace PSI.Models
{
    public class PlaylistSong
    {
        public Guid PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;

        public Guid SongId { get; set; }
        public Song Song { get; set; } = null!;

        public int Votes { get; set; }
        public DateTime AddedAt { get; set; }

        public PlaylistSong() { }

        public PlaylistSong(Song linkedSong, Playlist linkedPlaylist)
        {
            Song = linkedSong;
            SongId = linkedSong.Id;
            Playlist = linkedPlaylist;
            PlaylistId = linkedPlaylist.Id;
            AddedAt = DateTime.UtcNow;
        }

        public void Upvote()
        {
            Votes++;
        }
    }
}
