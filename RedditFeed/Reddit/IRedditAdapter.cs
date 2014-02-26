namespace RedditFeed.Reddit
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using RedditFeed.Client;

    public interface IRedditAdapter
    {
        Task<List<Listing>> GetListings(string subreddit, SortOrder sort, int limit);
    }
}