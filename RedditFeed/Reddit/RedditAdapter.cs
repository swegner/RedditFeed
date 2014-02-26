namespace RedditFeed.Reddit
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Ajax.Utilities;
    using RedditFeed.Client;

    public class RedditAdapter : IRedditAdapter
    {
        private readonly IListingsConverter _listingsConverter;

        public RedditAdapter(IListingsConverter listingsConverter)
        {
            this._listingsConverter = listingsConverter;
        }

        public async Task<List<Listing>> GetListings(string subreddit, SortOrder sort, int limit)
        {
            string url = string.Format("http://www.reddit.com/r/{0}/{1}.json?{2}", subreddit, sort.UrlPart, string.Join("&", sort.QueryParamters
                .Concat(new[] { new KeyValuePair<string, string>("limit", limit.ToStringInvariant()) })
                .Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value))));

            string json;
            using (HttpClient client = HttpClientFactory.Create())
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url) { Headers = { { "User-Agent", "RedditFeed by swegner2" } } })
            using (HttpResponseMessage response = await client.SendAsync(request))
            {
                json = await response.Content.ReadAsStringAsync();
            }

            return this._listingsConverter.Convert(json);
        }
    }
}