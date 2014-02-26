namespace RedditFeed.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using RedditFeed.Client;
    using RedditFeed.Reddit;

    [TestClass]
    public class ListingsConverterTests
    {
        private ListingsConverter _converter;

        /// <summary>
        /// Sample JSON response data.
        /// </summary>
        private static string TestData;

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            const string testDataResource = "RedditFeed.Tests.TestData.RedditResponse.js";
            
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream resourceStream = assembly.GetManifestResourceStream(testDataResource))
            using (StreamReader reader = new StreamReader(resourceStream))
            {
                TestData = reader.ReadToEnd();
            }
        }

        [TestInitialize]
        public void TestInitialize()
        {
            this._converter = new ListingsConverter();
        }

        [TestMethod]
        public void CanConvert()
        {
            // Act
            List<Listing> listings = this._converter.Convert(TestData);

            // Assert
            Assert.AreEqual(10, listings.Count);
        }

        [TestMethod]
        public void ConvertsTitle()
        {
            // Act
            List<Listing> listings = this._converter.Convert(TestData);

            // Assert
            Listing listing = listings[0];
            Assert.AreEqual(@"""Medicine Lake"" - Jasper National Park, Canada. [2048x1360][OS] photo by Christian Schmidt", listing.Title);
        }

        [TestMethod]
        public void ConvertsCreatedTime()
        {
            // Act
            List<Listing> listings = this._converter.Convert(TestData);

            // Assert
            Listing listing = listings[0];
            Assert.AreEqual(new DateTimeOffset(2014, 2, 25, 13, 57, 17, default(TimeSpan)), listing.CreatedTime);
        }

        [TestMethod]
        public void ConvertsDomain()
        {
            // Act
            List<Listing> listings = this._converter.Convert(TestData);

            // Assert
            Listing listing = listings[0];
            Assert.AreEqual("ppcdn.500px.org", listing.Domain);
        }

        [TestMethod]
        public void ConvertsUrl()
        {
            // Act
            List<Listing> listings = this._converter.Convert(TestData);

            // Assert
            Listing listing = listings[0];
            Assert.AreEqual(new Uri("http://ppcdn.500px.org/61859129/c66d0eeb208bacce173f486b6729347796aa0521/2048.jpg"), listing.Url);
        }

        [TestMethod]
        public void ConvertsScore()
        {
            // Act
            List<Listing> listings = this._converter.Convert(TestData);

            // Assert
            Listing listing = listings[0];
            Assert.AreEqual(2510, listing.Score);
        }
    }
}