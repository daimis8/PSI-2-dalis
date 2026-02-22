namespace PSI.Models
{
    public class Album : SongCollection
    {
        public string Artist { get; set; } = string.Empty;
        public int ReleaseYear { get; set; }
        public List<Song> Songs { get; set; } = new List<Song>();
        public Album() { }
        public Album(string name, string artist, int releaseYear)
        {
            Name = name;
            Artist = artist;
            ReleaseYear = releaseYear;

        }
        public override void IncreasePopularity()
        {
            Popularity++;
        }
        public void AddSong(Song song)
        {
            if (!Songs.Any(s => s.Id == song.Id))
            {
                Songs.Add(song);
            }
        }
        public List<Song> GetAllSongs() => Songs;

    }
}
