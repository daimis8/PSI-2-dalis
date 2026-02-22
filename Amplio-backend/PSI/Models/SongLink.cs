namespace PSI.Models
{
    public readonly struct SongLink
    {
        public string Value { get; }

        public SongLink(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Song link cannot be empty.", nameof(value));

            Value = NormalizeLink(value);
        }

        private static string NormalizeLink(string link)
        {
            if (string.IsNullOrWhiteSpace(link))
                return string.Empty;

            try
            {
                var uri = new Uri(link);

                if (uri.Host.Contains("youtu.be"))
                {
                    
                    string videoId = uri.AbsolutePath.Trim('/');
                    return $"https://www.youtube.com/watch?v={videoId}";
                }
                else if (uri.Host.Contains("youtube.com"))
                {
                    
                    var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    string videoId = query["v"];
                    if (!string.IsNullOrEmpty(videoId))
                        return $"https://www.youtube.com/watch?v={videoId}";
                }

                return link;
            }
            catch
            {
                return link;
            }
        }

        public override string ToString() => Value;
    }
}
