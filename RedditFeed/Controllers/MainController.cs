namespace RedditFeed.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.ServiceModel.Syndication;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web.Http;
    using System.Xml;
    using System.Xml.Linq;
    using RedditFeed.Client;
    using RedditFeed.Reddit;

    public class MainController : ApiController
    {
        private readonly IRedditAdapter _redditAdapter;

        public MainController(IRedditAdapter redditAdapter)
        {
            this._redditAdapter = redditAdapter;
        }

        public async Task<HttpResponseMessage> Get(string subreddit, string sort, string time)
        {
            HttpResponseMessage response;
            try
            {
                SyndicationFeed feed = await this.CreateFeed(subreddit, sort, time);
                response = this.CreateResponse(feed);
            }
            catch (Exception ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }

            return response;
        }

        private async Task<SyndicationFeed> CreateFeed(string subreddit, string sort, string time)
        {
            SortOrder sortOrder;
            if (!SortOrder.TryParse(sort, time, out sortOrder))
            {
                throw new ArgumentException(string.Format("Invalid sort order: {0}", string.Join(" ", new[] { sort, time })));
            }

            const int maxEntries = 30; // TDOO: make configurable
            List<Listing> listings = await this._redditAdapter.GetListings(subreddit, sortOrder, maxEntries);

            string title = string.Join(" ", "reddit", subreddit, sort, time);

            SyndicationFeed feed = new SyndicationFeed(title, title, Request.RequestUri)
            {
                LastUpdatedTime = DateTimeOffset.UtcNow,
                Items = listings
                    .Where(this.Validate)
                    .Select(this.CreateSyndicationItem)
                    .Where(this.PostValidate)
                    .ToList(),
            };

            feed.ElementExtensions.Add("pubDate", string.Empty, DateTimeOffset.UtcNow.ToString("r"));
            return feed;
        }

        private bool Validate(Listing listing)
        {
            const int minWidth = 1024;
            const int minHeight = 768;

            const float minAspectRatio = 1.0f;
            const float maxAspectRatio = 2.0f;

            Match match = Regex.Match(listing.Title, @"[\[(]\s*(?<width>\d+)\s*x\s*(?<height>\d+)\s*[\])]");
            bool valid;
            if (match.Success)
            {
                int width = int.Parse(match.Groups["width"].Value);
                int height = int.Parse(match.Groups["height"].Value);
                float aspectRatio = ((float)width) / height;

                valid =
                    width >= minWidth &&
                    height >= minHeight &&
                    minAspectRatio <= aspectRatio && 
                    aspectRatio <= maxAspectRatio;
            }
            else
            {
                valid = false;
            }

            return valid;
        }

        private SyndicationItem CreateSyndicationItem(Listing listing)
        {
            SyndicationItem item = new SyndicationItem(listing.Title, listing.Title, listing.Url)
            {
                PublishDate = listing.CreatedTime
            };
            item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("url", listing.Url), new XAttribute("type", "image/jpeg")));

            return item;
        }

        private bool PostValidate(SyndicationItem item)
        {
            string url = item.ElementExtensions
                .Select(e => e.GetObject<XElement>())
                .Where(x => x.Name == "enclosure")
                .Select(x => x.Attribute("url").Value)
                .First();

            return Regex.IsMatch(url, @"\.((jpg)|(jpeg)|(png))$");
        }

        private HttpResponseMessage CreateResponse(SyndicationFeed feed)
        {
            HttpResponseMessage response;
            using (MemoryStream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms))
                {
                    feed.SaveAsRss20(writer);
                    writer.Flush();
                    byte[] buffer = ms.ToArray();
                    string output = Encoding.UTF8.GetString(buffer, 0, buffer.Length);

                    response = Request.CreateResponse();
                    response.Content = new StringContent(output, Encoding.UTF8, "application/rss+xml");
                }
            }
            return response;
        }
    }
}
