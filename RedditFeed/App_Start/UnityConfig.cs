namespace RedditFeed
{
    using System.Web.Http;
    using Microsoft.Practices.Unity;
    using RedditFeed.Client;
    using RedditFeed.Reddit;
    using Unity.WebApi;

    public static class UnityConfig
    {
        public static UnityContainer Container { get; private set; }

        public static void RegisterComponents()
        {
            Container = new UnityContainer();
            Container
                .RegisterType<IRedditAdapter, RedditAdapter>()
                .RegisterType<IListingsConverter, ListingsConverter>();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(Container);
        }
    }
}