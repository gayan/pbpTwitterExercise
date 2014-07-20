using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using pbpTwitterExercise.Models;
using pbpTwitterExercise.Models.Correspondents;
using pbpTwitterExercise.Models.Services;

namespace pbpTwitterExercise.Test
{
    [TestFixture]
    public class TwitterServiceUnitTest
    {
        private TwitterService twitterService;

        private readonly DateTime twoWeeksAgo = DateTime.Now.AddDays(-14);

        private readonly IDictionary<string, ExpectedFeedData> expectedFeedData = new Dictionary<string, ExpectedFeedData>();

        private const int MaxTwitterHandles = 5;
            
        [SetUp]
        public void Init()
        {
            twitterService = new TwitterService();
            const string mockTwitterBearerToken = "fakeBearerToken";
            var twitterCorrespondentMock = new Mock<ISocialCorrespondent>();
            twitterCorrespondentMock.Setup(x => x.GetBearerToken(It.IsAny<string>(), It.IsAny<string>())).Returns(mockTwitterBearerToken);
            
            const string accountNamePrefix = "@account";
            expectedFeedData.Clear();
            for (int i = 0; i < MaxTwitterHandles; i++)
            {
                int accountNumber = i;
                twitterCorrespondentMock.Setup(x => x.GetTimeline(accountNamePrefix + accountNumber, mockTwitterBearerToken)).Returns(CreateTestFeedData(accountNamePrefix, i));
            }

            twitterService.TwitterCorrespondent = twitterCorrespondentMock.Object;
        }

        private IList<FeedItem> CreateTestFeedData(string accountName, int suffix)
        {
            var r = new Random();
            var feedItems = new List<FeedItem>();
            var tweetsWithinDateRange = 0;
            var mentions = 0;
            var filteredFeedItems = new List<FeedItem>();
            for (int i = 0; i < 200; i++)
            {
                var randomPostDate = DateTime.Now.AddDays(r.Next(-30, 0));

                var feedItem = new FeedItem
                {
                    AccountName = accountName + suffix,
                    Text = "@account" + i + " post number " + i,
                    PostedDateTime = randomPostDate
                };

                feedItems.Add(feedItem);

                if (randomPostDate >= twoWeeksAgo)
                {
                    tweetsWithinDateRange++;
                    filteredFeedItems.Add(feedItem);

                    // when the suffix doesn't match the current account number i
                    // and current i is less than the max Twitter Handles, it's a mention.
                    if (i != suffix)
                    {
                        mentions++;
                    }
                }
            }

            expectedFeedData.Add(accountName + suffix, new ExpectedFeedData
            {
                TotalFeedItems = tweetsWithinDateRange,
                TotalExternalReferences = mentions,
                AllFeedItems = feedItems,
                FilteredFeedItems = filteredFeedItems
            });

            return feedItems;
        }

        [Test]
        public void TestGetAggregateFeedReturnsCorrectTotalCount()
        {
            var aggregateFeed = GetAggregateFeedFromTestData();

            Assert.NotNull(aggregateFeed);
            Assert.AreEqual(expectedFeedData["@account0"].TotalFeedItems, aggregateFeed.MetaData["@account0"].TotalFeedItems);
            Assert.AreEqual(expectedFeedData["@account1"].TotalFeedItems, aggregateFeed.MetaData["@account1"].TotalFeedItems);
            Assert.AreEqual(expectedFeedData["@account2"].TotalFeedItems, aggregateFeed.MetaData["@account2"].TotalFeedItems);
        }

        [Test]
        public void TestGetAggregateFeedReturnsCorrectTotalRefrenceCount()
        {
            var aggregateFeed = GetAggregateFeedFromTestData();

            Assert.NotNull(aggregateFeed);
            Assert.AreEqual(expectedFeedData["@account0"].TotalExternalReferences, aggregateFeed.MetaData["@account0"].TotalExternalReferences);
            Assert.AreEqual(expectedFeedData["@account1"].TotalExternalReferences, aggregateFeed.MetaData["@account1"].TotalExternalReferences);
            Assert.AreEqual(expectedFeedData["@account2"].TotalExternalReferences, aggregateFeed.MetaData["@account2"].TotalExternalReferences);
        }
        
        [Test]
        public void TestGetAggregateFeedItemsCountMatchesItemsInFeed()
        {
            var aggregateFeed = GetAggregateFeedFromTestData();

            Assert.NotNull(aggregateFeed);
            Assert.AreEqual(expectedFeedData["@account0"].FilteredFeedItems.Count, aggregateFeed.MetaData["@account0"].TotalFeedItems);
            Assert.AreEqual(expectedFeedData["@account1"].FilteredFeedItems.Count, aggregateFeed.MetaData["@account1"].TotalFeedItems);
            Assert.AreEqual(expectedFeedData["@account2"].FilteredFeedItems.Count, aggregateFeed.MetaData["@account2"].TotalFeedItems);
        }
        
        [Test]
        public void TestGetAggregateFeedContainsAllExpectedFeedItems()
        {
            // get the results from the service
            var aggregateFeed = twitterService.GetAggregateFeed(new[]
            {
                "@account0",
                "@account1"
            }, twoWeeksAgo);

            // merge expected data into a list
            var concatenatedFeed = new List<FeedItem>();
            concatenatedFeed.AddRange(expectedFeedData["@account0"].FilteredFeedItems);
            concatenatedFeed.AddRange(expectedFeedData["@account1"].FilteredFeedItems);

            // does not test for the correct order here, that is a separate test.
            Assert.NotNull(aggregateFeed);
            Assert.That(aggregateFeed.FeedItems.ToArray(), Is.EquivalentTo(concatenatedFeed.OrderByDescending(o=>o.PostedDateTime).ToArray()));
        }

        [Test]
        public void TestGetAggregateFeedItemsAreSorted()
        {
            var aggregateFeed = GetAggregateFeedFromTestData();

            var previousDate = aggregateFeed.FeedItems[0].PostedDateTime;
            for (int i = 1; i < aggregateFeed.FeedItems.Count; i++)
            {
                var currentDate = aggregateFeed.FeedItems[i].PostedDateTime;
                Assert.GreaterOrEqual(previousDate, currentDate); // going down the list, dates are descending.
                previousDate = currentDate;
            }
        }

        private AggregateFeed GetAggregateFeedFromTestData()
        {
            var aggregateFeed = twitterService.GetAggregateFeed(new[]
            {
                "@account0",
                "@account1",
                "@account2"
            }, twoWeeksAgo);
            return aggregateFeed;
        }
    }

    internal class ExpectedFeedData : FeedMetaData
    {
        public IList<FeedItem> AllFeedItems { get; set; }
        public IList<FeedItem> FilteredFeedItems { get; set; }
    }
}
