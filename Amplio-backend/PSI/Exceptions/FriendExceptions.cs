namespace PSI.Exceptions
{
    public class SelfFriendRequestException : Exception
    {
        public SelfFriendRequestException() : base("Cannot send a friend request to yourself") { }
    }

    public class DuplicateFriendRequestException : Exception
    {
        public DuplicateFriendRequestException() : base("A pending friend request already exists between these users") { }
    }

    public class AlreadyFriendsException : Exception
    {
        public AlreadyFriendsException() : base("Users are already friends") { }
    }

    public class BlockedRelationshipException : Exception
    {
        public BlockedRelationshipException() : base("Cannot perform this action because a block exists between the users") { }
    }

    public class FriendRequestRateLimitException : Exception
    {
        public FriendRequestRateLimitException() : base("Friend request rate limit exceeded (20 per hour)") { }
    }

    public class FriendRequestNotFoundException : Exception
    {
        public FriendRequestNotFoundException() : base("Friend request not found") { }
    }

    public class FriendshipNotFoundException : Exception
    {
        public FriendshipNotFoundException() : base("Friendship not found") { }
    }

    public class NotFriendsException : Exception
    {
        public NotFriendsException() : base("The target user is not a friend") { }
    }
}
