using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Moq;
using NUnit.Framework;
using pbpTwitterExercise.Controllers;
using pbpTwitterExercise.Models;
using pbpTwitterExercise.Models.Correspondents;
using pbpTwitterExercise.Models.Services;

namespace pbpTwitterExercise.Test
{
    [TestFixture]
    public class IntegrationTest
    {
        [Test]
        public void TestTwitterControllerResponse()
        {
            /* NOTE: Since our controller has no logic in it
             * we will not mock the TwitterService, and unit test the controller.
             * Instead we will do an integration test.
             */
            var controller = new TwitterController
            {
                TwitterService = new TwitterService
                {
                    TwitterCorrespondent = new TwitterCorrespondent()
                }
            };

            var result = controller.Index() as JsonResult;

            Assert.NotNull(result);

            var aggregateFeed = result.Data as AggregateFeed;

            Assert.NotNull(aggregateFeed);

            var aggregateFeedCount = aggregateFeed.MetaData.Sum(metaData => metaData.Value.TotalFeedItems);

            Assert.True(aggregateFeed.FeedItems.Count > 0);
            Assert.AreEqual(aggregateFeed.FeedItems.Count, aggregateFeedCount);
        }

        // Similar to above, but mocking the dependencies. So this is a unit test
        [Test]
        public void TestTwitterControllerUnitTest()
        {
            var expectedAggregateFeed = new AggregateFeed
            {
                FeedItems = new[]
                {
                    new FeedItem
                    {
                        AccountName = "@account1",
                        PostedDateTime = DateTime.Now,
                        Text = "@another1 @another2 @another3 blah blah blah"
                    }, new FeedItem
                    {
                        AccountName = "@account2",
                        PostedDateTime = DateTime.Now,
                        Text = "blah blah blah"
                    }, new FeedItem
                    {
                        AccountName = "@account3",
                        PostedDateTime = DateTime.Now,
                        Text = "blah blah blah blah blah blah"
                    }

                },
                MetaData = new Dictionary<string, FeedMetaData>
                {
                    {
                        "@account1", new FeedMetaData
                        {
                            TotalExternalReferences = 3,
                            TotalFeedItems = 1
                        }
                    },
                    {
                        "@account2", new FeedMetaData
                        {
                            TotalExternalReferences = 2,
                            TotalFeedItems = 1
                        }
                    },
                    {
                        "@account3", new FeedMetaData
                        {
                            TotalExternalReferences = 1,
                            TotalFeedItems = 1
                        }
                    }
                }
            };

            var socialServiceMock = new Mock<ISocialService>();
            socialServiceMock.Setup(x => x.GetAggregateFeed(It.IsAny<IList<string>>(), It.IsAny<DateTime>())).Returns(expectedAggregateFeed);

            var controller = new TwitterController
            {
                TwitterService = socialServiceMock.Object // use the mock object
            };

            var result = controller.Index() as JsonResult;

            Assert.NotNull(result);
            Assert.AreEqual(expectedAggregateFeed, result.Data);
        }
    }
}
