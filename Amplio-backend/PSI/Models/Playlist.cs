using System.Collections;
namespace PSI.Models
{
    public class Playlist : SongCollection, IEnumerable<PlaylistSong>
    {
        public Guid OwnerId { get; set; }
        public Guid? CurrentSongId { get; set; }
        public Song? CurrentSong { get; set; } = null!;
        public List<PlaylistSong> Songs { get; set; } = new List<PlaylistSong>();

        public bool IsPublic { get; set; } = false;
        public int VisitCount { get; set; } = 0;
        public Playlist() { }
        public Playlist(string name, bool isPublic, Guid ownerId)
        {
            Name = name;
            IsPublic = isPublic;
            OwnerId = ownerId;
        }

        public override void IncreasePopularity()
        {
            VisitCount++;
            Popularity = VisitCount;
        }
        public void AddSong(Song songToAdd)
        {
            bool songAlreadyExists = Songs.Any(playlistSong => playlistSong.SongId == songToAdd.Id);
            if (!songAlreadyExists)
            {
                var newPlaylistSong = new PlaylistSong(songToAdd, this);
                Songs.Add(newPlaylistSong);
            }
        }

        public List<PlaylistSong> GetAllSongs() =>
            Songs.OrderByDescending(playlistSong => playlistSong.Votes).ThenBy(playlistSong => playlistSong.AddedAt).ToList();

        public PlaylistSong? GetSongById(Guid songId) =>
            Songs.FirstOrDefault(playlistSong => playlistSong.SongId == songId);

        public bool DeleteSong(Guid songId)
        {
            var playlistSongToRemove = GetSongById(songId);
            if (playlistSongToRemove == null) return false;
            Songs.Remove(playlistSongToRemove);
            return true;
        }

        public void UpvoteSong(Guid songId)
        {
            var playlistSongToUpvote = GetSongById(songId);
            if (playlistSongToUpvote != null) playlistSongToUpvote.Upvote();
        }

        public IEnumerator<PlaylistSong> GetEnumerator() => Songs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}

