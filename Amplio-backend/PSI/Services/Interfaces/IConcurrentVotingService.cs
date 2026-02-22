namespace PSI.Services.Interfaces
{
    public interface IConcurrentVotingService
    {
        Task UpvoteAsync(Guid playlistId, Guid songId);
        Task<int> GetVotesAsync(Guid playlistId, Guid songId);
        Task InitializeCacheAsync();
    }
}