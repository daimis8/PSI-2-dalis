namespace PSI.Models
{
    public class PlaylistInvitation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PlaylistId { get; set; }
        public Guid InviteeId { get; set; }
        public Guid InviterId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Playlist? Playlist { get; set; }
        public User? Invitee { get; set; }
    }
}
