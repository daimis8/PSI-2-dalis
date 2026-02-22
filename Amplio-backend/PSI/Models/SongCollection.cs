namespace PSI.Models
{
    public abstract class SongCollection
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty;
        public int Popularity { get; set; } = 0;
        public abstract void IncreasePopularity();
    }
}
