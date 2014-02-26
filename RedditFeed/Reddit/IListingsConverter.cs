namespace RedditFeed.Reddit
{
    using System.Collections.Generic;
    using RedditFeed.Client;

    public interface IListingsConverter
    {
        List<Listing> Convert(string json);
    }
}