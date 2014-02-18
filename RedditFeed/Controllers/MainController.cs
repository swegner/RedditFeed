using System;
using System.Collections.Generic;
using System.Globalization;
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

namespace RedditFeed.Controllers
{
    public class MainController : ApiController
    {
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
            string url = string.Format("http://www.reddit.com/r/{0}/{1}/.rss{2}", subreddit, sort,
                string.IsNullOrEmpty(time) ? string.Empty : string.Format("?sort={0}&t={1}", sort, time));

            string content;
            using (HttpClient httpClient = new HttpClient())
            {
                content = await httpClient.GetStringAsync(url);
            }

            XDocument xml = XDocument.Parse(content);

            string title = string.Join(" ", "reddit", subreddit, sort, time);

            SyndicationFeed feed = new SyndicationFeed(title, title, Request.RequestUri)
            {
                LastUpdatedTime = DateTimeOffset.UtcNow,
                Items = xml.Descendants("item")
                    .Where(this.Validate)
                    .Select(this.CreateSyndicationItem)
                    .Where(this.PostValidate)
                    .ToList(),
            };

            feed.ElementExtensions.Add("pubDate", string.Empty, DateTimeOffset.UtcNow.ToString("r"));
            return feed;
        }

        private bool Validate(XElement item)
        {
            const int minWidth = 1024;
            const int minHeight = 768;

            const float minAspectRatio = 1.0f;
            const float maxAspectRatio = 2.0f;

            Match match = Regex.Match(item.Element("title").Value, @"[\[(]\s*(?<width>\d+)\s*x\s*(?<height>\d+)\s*[\])]");
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

        private SyndicationItem CreateSyndicationItem(XElement element)
        {
            string description = element.Element("copyrightsource") != null ?
                element.Element("copyrightsource").Value :
                null;

            string title = element.Element("title").Value;
            Uri link = new Uri(element.Element("link").Value);
            Uri enclosure = XDocument.Parse(element.Element("description").Value).Descendants("a")
                .Where(x => x.Value == "[link]")
                .Select(x => new Uri(x.Attribute("href").Value))
                .First();

            SyndicationItem item = new SyndicationItem(title, title, link)
            {
                PublishDate = DateTimeOffset.Parse(element.Element("pubDate").Value)
            };
            item.ElementExtensions.Add(new XElement("enclosure", new XAttribute("url", enclosure), new XAttribute("type", "image/jpeg")));

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
