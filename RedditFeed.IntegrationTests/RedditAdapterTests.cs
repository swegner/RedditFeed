namespace RedditFeed.Tests
{
    using System.Collections.Generic;
    using Microsoft.Practices.Unity;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RedditFeed.Client;
    using RedditFeed.Reddit;

    [TestClass]
    public class RedditAdapterTests
    {
        private IRedditAdapter _adapter;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            UnityConfig.RegisterComponents();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this._adapter = UnityConfig.Container.Resolve<IRedditAdapter>();
        }

        [TestMethod, TestCategory("Integration"), Description("Verify that GetListings returns successfully.")]
        public void GetListingWorks()
        {
            // Act
            List<Listing> listings = this._adapter.GetListings("earthporn", SortOrder.Hot, 10).Result;

            // Assert
            Assert.AreEqual(10, listings.Count);
        }
    }
}
