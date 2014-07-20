using System.Configuration;
using NUnit.Framework;
using pbpTwitterExercise.Models.Correspondents;
using pbpTwitterExercise.Models.Exceptions;

namespace pbpTwitterExercise.Test
{
    // Intentionally not naming this class "Unit Test" becase it calls web services.

    [TestFixture]
    public class TwitterCorrespondentTest
    {
        private ISocialCorrespondent twitterCorrespondent;
        private readonly string consumerKey = ConfigurationManager.AppSettings["Twitter.ConsumerKey"];
        private readonly string consumerSecret = ConfigurationManager.AppSettings["Twitter.ConsumerSecret"];
        
        [SetUp]
        public void Init()
        {
            twitterCorrespondent = new TwitterCorrespondent();
        }

        [Test]
        public void TestGetBearerTokenIdealCase()
        {
            var bearerToken = twitterCorrespondent.GetBearerToken(consumerKey, consumerSecret);

            Assert.IsNotNullOrEmpty(bearerToken);
        }

        [Test]
        public void TestGetBearerTokenNullConsumerKey()
        {
            var bearerToken = twitterCorrespondent.GetBearerToken(null, consumerSecret);

            Assert.IsNull(bearerToken);
        }

        [Test]
        public void TestGetBearerTokenEmptyConsumerKey()
        {
            var bearerToken = twitterCorrespondent.GetBearerToken("", consumerSecret);

            Assert.IsNull(bearerToken);
        }

        [Test]
        public void TestGetBearerTokenNullConsumerSecret()
        {
            var bearerToken = twitterCorrespondent.GetBearerToken(consumerKey, null);

            Assert.IsNull(bearerToken);
        }

        [Test]
        public void TestGetBearerTokenEmptyConsumerSecret()
        {
            var bearerToken = twitterCorrespondent.GetBearerToken(consumerKey, "");

            Assert.IsNull(bearerToken);
        }

        [Test]
        [ExpectedException]
        public void TestGetBearerTokenWithInvalidConsumerKeyAndConsumerSecret()
        {
            twitterCorrespondent.GetBearerToken("invalidConsumerKey", "invalidConsumerSecret");
        }

        // Not a unit test, when it actually calls out to the web service.
        [Test]
        public void TestGetTimelineIdealCase()
        {
            var bearerToken = twitterCorrespondent.GetBearerToken(consumerKey, consumerSecret);
            var aggregateFeed = twitterCorrespondent.GetTimeline("@pay_by_phone", bearerToken);

            Assert.IsNotNull(aggregateFeed);
            Assert.IsNotEmpty(aggregateFeed); // We're counting on our knowledge that this account has tweets.
        }

        [Test]
        public void TestGetTimelineWithEmptyUsername()
        {
            var aggregateFeed = twitterCorrespondent.GetTimeline("");
            
            Assert.IsNull(aggregateFeed);
        }

        [Test]
        public void TestGetTimelineWithNullUsername()
        {
            var aggregateFeed = twitterCorrespondent.GetTimeline(null);

            Assert.IsNull(aggregateFeed);
        }

        // Not a unit test, when it actually calls out to the web service.
        [Test]
        [ExpectedException(typeof(SocialUserNotFoundException))]
        public void TestGetTimelineOfUnknownUser()
        {
            var bearerToken = twitterCorrespondent.GetBearerToken(consumerKey, consumerSecret);
            twitterCorrespondent.GetTimeline("@8cbbdec8-a755-4bfa-aaa3-ee2620772222", bearerToken); // a user with this guid as a username should not exist.
        }

        // Not a unit test, when it actually calls out to the web service.
        [Test]
        [ExpectedException]
        public void TestGetTimeLineWithInvalidBearerToken()
        {
            twitterCorrespondent.GetTimeline("@pay_by_phone", "invalidBearerToken");
        }
    }
}
