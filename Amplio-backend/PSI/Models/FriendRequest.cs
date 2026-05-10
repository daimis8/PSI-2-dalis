namespace PSI.Models
{
    public class FriendRequest
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SenderId { get; set; }
        public Guid ReceiverId { get; set; }
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public User? Sender { get; set; }
        public User? Receiver { get; set; }
    }
}
