namespace PSI.Exceptions
{
    public class PlaylistOperationException : Exception
    {
        public Guid PlaylistId { get; }

        public PlaylistOperationException(string message, Guid playlistId)
            : base(FormatMessage(message, playlistId))
        {
            PlaylistId = playlistId;
        }

        public PlaylistOperationException(string message, Guid playlistId, Exception innerException)
            : base(FormatMessage(message, playlistId), innerException)
        {
            PlaylistId = playlistId;
        }

        private static string FormatMessage(string message, Guid playlistId)
        {
            return $"{message} (Playlist ID: '{playlistId}')";
        }
    }
}