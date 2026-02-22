using PSI.Models;

namespace PSI.Extensions
{
    public static class PlaylistExtensions
    {
        
        public static bool UpvoteSongById(this Playlist playlist, Guid songId)
        {
            var playlistSong = playlist.GetSongById(songId);
            if (playlistSong == null)
                return false;

            playlistSong.Upvote();
            return true;
        }
    }
}
