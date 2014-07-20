using System;
using System.Collections.Generic;

namespace pbpTwitterExercise.Models
{
    public class AggregateFeed
    {
        private IList<FeedItem> feedItems = new List<FeedItem>();
        public IList<FeedItem> FeedItems
        {
            get { return feedItems; }
            set { feedItems = value; }
        }

        private IDictionary<string, FeedMetaData> metaData = new Dictionary<string, FeedMetaData>();
        public IDictionary<string, FeedMetaData> MetaData
        {
            get { return metaData; }
            set { metaData = value; }
        }
    }

    /* NOTE: I like having these classes in the same file here because they are
     * both relatd to the AggregateFeed class above.
     * Depending on coding style, some may think it is harder to find here. 
     * It would be trivial to move them into their own files.
     */

    public class FeedMetaData
    {
        public int TotalFeedItems { get; set; }
        public int TotalExternalReferences { get; set; }
    }

    public class FeedItem
    {
        public string AccountName { get; set; }
        public string Text { get; set; }
        public DateTime PostedDateTime { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1} AT {2}", AccountName, Text, PostedDateTime);
        }
    }
}
