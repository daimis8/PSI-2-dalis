namespace PSI.DTOs
{
    public class LeaderboardResponseDto<T>
    {
        public List<T> LeaderboardItems { get; set; } = new List<T>();
    }
}