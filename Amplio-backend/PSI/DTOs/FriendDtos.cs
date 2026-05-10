using PSI.Models;

namespace PSI.DTOs
{
    public record UserSearchResultDto(Guid Id, string Username);

    public record FriendDto(Guid Id, string Username, DateTime FriendshipCreatedAt);

    public record FriendRequestDto(
        Guid Id,
        Guid SenderId,
        string SenderUsername,
        Guid ReceiverId,
        string ReceiverUsername,
        FriendRequestStatus Status,
        DateTime SentAt);

    public record SendFriendRequestDto(Guid ReceiverId);
}
