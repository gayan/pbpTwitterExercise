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

        private const string MockTwitterBearerToken = "fakeBearerToken";

        private const int MaxTwitterHandles = 5;

        private const int MaxFeedsToAggregate = 3;

        private readonly IList<string> twitterHandles = new List<string>();

        [SetUp]
        public void Init()
        {
            twitterService = new TwitterService();

            // Mock the ISocialCorrespondent.
            var twitterCorrespondentMock = new Mock<ISocialCorrespondent>();
            twitterCorrespondentMock.Setup(x => x.GetBearerToken(It.IsAny<string>(), It.IsAny<string>())).Returns(MockTwitterBearerToken);

            // Generate test twitter feeds, and setup return values from mock twitterCorrespondent.
            const string accountNamePrefix = "@account";
            for (int i = 0; i < MaxTwitterHandles; i++)
            {
                var accountName = accountNamePrefix + i;
                if (i < MaxFeedsToAggregate)
                {
                    twitterHandles.Add(accountName);
                }

                twitterCorrespondentMock.Setup(x => x.GetTimeline(accountName, MockTwitterBearerToken)).Returns(CreateTestFeedData(accountNamePrefix, i));
            }

            //
            twitterService.TwitterCorrespondent = twitterCorrespondentMock.Object;
        }

        [TearDown]
        public void Cleanup()
        {
            expectedFeedData.Clear();
            twitterHandles.Clear();
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
                var randomPostDate = DateTime.Now.AddDays(r.Next(-30, 0)); // random date between today and 30 days ago.

                var feedItem = new FeedItem
                {
                    AccountName = accountName + suffix,
                    Text = "hello @account" + i + " this is post number " + i + ", blah blah",
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
            foreach (var twitterHandle in twitterHandles)
            {
                Assert.AreEqual(expectedFeedData[twitterHandle].TotalFeedItems, aggregateFeed.MetaData[twitterHandle].TotalFeedItems);
            }
        }

        [Test]
        public void TestGetAggregateFeedReturnsCorrectTotalRefrenceCount()
        {
            var aggregateFeed = GetAggregateFeedFromTestData();

            Assert.NotNull(aggregateFeed);
            foreach (var twitterHandle in twitterHandles)
            {
                Assert.AreEqual(expectedFeedData[twitterHandle].TotalExternalReferences, aggregateFeed.MetaData[twitterHandle].TotalExternalReferences);
            }
        }

        [Test]
        public void TestGetAggregateFeedItemsCountMatchesItemsInFeed()
        {
            var aggregateFeed = GetAggregateFeedFromTestData();

            Assert.NotNull(aggregateFeed);
            foreach (var twitterHandle in twitterHandles)
            {
                Assert.AreEqual(expectedFeedData[twitterHandle].FilteredFeedItems.Count, aggregateFeed.MetaData[twitterHandle].TotalFeedItems);
            }
        }

        [Test]
        public void TestGetAggregateFeedContainsAllExpectedFeedItems()
        {
            // get the results from the service
            var aggregateFeed = GetAggregateFeedFromTestData();

            // merge expected data into a list.
            var concatenatedFeed = new List<FeedItem>();
            foreach (var twitterHandle in twitterHandles)
            {
                concatenatedFeed.AddRange(expectedFeedData[twitterHandle].FilteredFeedItems);
            }

            Assert.NotNull(aggregateFeed);
            Assert.That(aggregateFeed.FeedItems.ToArray(), Is.EqualTo(concatenatedFeed.OrderByDescending(o => o.PostedDateTime).ToArray()));
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

        [Test]
        public void TestGetAggregateFeedItemMentionsRegularExpression()
        {
            string name1 = "@com__h4nd_";
            string name2 = "@JANDLE";
            string name3 = "@tW_tw_2";
            var handles = new[] { name1, name2, name3 };

            var tcMock = new Mock<ISocialCorrespondent>();
            tcMock.Setup(x => x.GetBearerToken(It.IsAny<string>(), It.IsAny<string>())).Returns(MockTwitterBearerToken);
            tcMock.Setup(x => x.GetTimeline(name1, MockTwitterBearerToken)).Returns(new[]
            {
                new FeedItem
                {
                    AccountName = name1,
                    PostedDateTime = DateTime.Now,
                    Text = "hello there @so_ot_PE, tweet at me anytime " + name1 + name2 // 2 mentions. One is my own.
                }
            });
            twitterService.TwitterCorrespondent = tcMock.Object;

            var aggregateFeed = twitterService.GetAggregateFeed(handles, DateTime.Now.AddYears(-1));

            Assert.AreEqual(2, aggregateFeed.MetaData[name1].TotalExternalReferences);
        }

        private AggregateFeed GetAggregateFeedFromTestData()
        {
            var aggregateFeed = twitterService.GetAggregateFeed(twitterHandles, twoWeeksAgo);
            return aggregateFeed;
        }
    }

    internal class ExpectedFeedData : FeedMetaData
    {
        public IList<FeedItem> AllFeedItems { get; set; }
        public IList<FeedItem> FilteredFeedItems { get; set; }
    }
}
