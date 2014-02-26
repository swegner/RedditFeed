namespace RedditFeed.Reddit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json.Linq;
    using RedditFeed.Client;

    public class ListingsConverter : IListingsConverter
    {
        private static DateTimeOffset UnixEpoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, default(TimeSpan));

        public List<Listing> Convert(string json)
        {
            JObject parsed = JObject.Parse(json);
            return parsed["data"]["children"]
                .Select(j =>
                {
                    JToken data = j["data"];
                    return new Listing
                    {
                        Title = data.Value<string>("title"),
                        CreatedTime = UnixEpoch + TimeSpan.FromSeconds(data.Value<double>("created_utc")),
                        Url = new Uri(data.Value<string>("url")),
                        Domain = data.Value<string>("domain"),
                        Score = data.Value<int>("score"),
                    };
                })
                .ToList();
        }
    }
}