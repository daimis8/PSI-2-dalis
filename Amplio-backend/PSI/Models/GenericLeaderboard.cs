namespace PSI.Models
{
    public class GenericLeaderboard<T> where T : notnull, SongCollection
    {
        public List<T> LeaderboardItems { get; set; } = new List<T>();
        public void AddSongCollection(T collection)
        {
            if (collection != null && !LeaderboardItems.Any(c=>c.Id == collection.Id)) 
                LeaderboardItems.Add(collection);
        }

        public void SortByPopularity()
        {
            LeaderboardItems = LeaderboardItems
                .OrderByDescending(c => c.Popularity)
                .ThenBy(c => c.Name)
                .ToList();
        }

        public List<T> GetSortedByPopularity()
        {
            SortByPopularity();
            return LeaderboardItems;
        }

        public void RemoveSongList(Guid id)
        {
            var collection = LeaderboardItems.FirstOrDefault(c => c.Id == id);
            if (collection != null)
            {
                LeaderboardItems.Remove(collection);
            }
        }
        public U GetPropertyOfMostPopular<U>(Func<T, U> selector)
        {
            if (LeaderboardItems.Count == 0)
                throw new InvalidOperationException("Leaderboard is empty.");

            return LeaderboardItems
                .OrderByDescending(c => c.Popularity)
                .Select(selector)
                .First();
        }
    }
}
