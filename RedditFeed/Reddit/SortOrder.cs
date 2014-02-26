namespace RedditFeed.Reddit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public sealed class SortOrder
    {
        private static readonly List<SortOrder> Sorts;
 
        static SortOrder()
        {
            Sorts = new List<SortOrder>();


            Hot = new SortOrder("hot");
            New = new SortOrder("new");

            IEnumerable<SortOrder> sorts = (new[] { "top", "controversial" }
                .SelectMany(_ => new[] { "hour", "day", "week", "month", "year", "all" }, (n, s) => new SortOrder(n, s)))
                .Concat(new[] { Hot, New });

            foreach (SortOrder sort in sorts)
            {
                Sorts.Add(sort);
            }
        }

        /// <summary>
        /// The primary name of the sort.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The sub-sort name, or null if none exists.
        /// </summary>
        public string SubSort { get; set; }

        /// <summary>
        /// The URL part to append to the API endpoint.
        /// </summary>
        public string UrlPart { get; private set; }

        /// <summary>
        /// Set of query parameters to append to the URL.
        /// </summary>
        public IDictionary<string, string> QueryParamters { get; private set; }
        /// <summary>
        /// Construct a new <see cref="SortOrder"/> instance with a subsort.
        /// </summary>
        /// <param name="name">The name for the ordering.</param>
        /// <param name="subSort">The sub-sort name, or null if none exists.</param>
        private SortOrder(string name, string subSort = null)
        {
            this.Name = name;
            this.SubSort = subSort;
            this.UrlPart = name.ToLowerInvariant();

            this.QueryParamters = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(subSort))
            {
                this.QueryParamters.Add("t", subSort.ToLowerInvariant());
            }
        }

        public static List<SortOrder> All
        {
            get { return Sorts; }
        }

        public static SortOrder Hot { get; private set; }
        public static SortOrder New { get; private set; }

        public static bool TryParse(string sortName, string subSort, out SortOrder sortOrder)
        {
            string translatedSubSort = string.Equals("today", subSort, StringComparison.InvariantCultureIgnoreCase) 
                ? "day" 
                : subSort;

            sortOrder = Sorts.FirstOrDefault(s => string.Equals(s.Name, sortName, StringComparison.InvariantCultureIgnoreCase)
                && string.Equals(s.SubSort, translatedSubSort));

            return sortOrder != null;
        }
    }
}