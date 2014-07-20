using System.Collections.Generic;

namespace pbpTwitterExercise.Models.Correspondents
{
    public interface ISocialCorrespondent
    {
        string GetBearerToken(string consumerKey, string consumerSecret);

        IList<FeedItem> GetTimeline(string user, string bearerToken = null);
    }
}
