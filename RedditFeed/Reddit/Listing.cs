namespace RedditFeed.Client
{
    using System;

    public class Listing
    {
        public string Title { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public string Domain { get; set; }
        public Uri Url { get; set; }
        public int Score { get; set; }
    }
}