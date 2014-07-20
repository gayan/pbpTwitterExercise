using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using Microsoft.Practices.Unity;
using pbpTwitterExercise.Models.Correspondents;

namespace pbpTwitterExercise.Models.Services
{
    public class TwitterService : ISocialService
    {
        [Dependency("twitter")]
        public ISocialCorrespondent TwitterCorrespondent { get; set; }

        public readonly string ConsumerKey = WebConfigurationManager.AppSettings["Twitter.ConsumerKey"];
        public readonly string ConsumerSecret = WebConfigurationManager.AppSettings["Twitter.ConsumerSecret"];

        public AggregateFeed GetAggregateFeed(IList<string> twitterHandles, DateTime fromDateTime)
        {
            // The service calls we are making to twitter only requires a bearer token, to
            // make calls on behalf of the app.
            var bearerToken = TwitterCorrespondent.GetBearerToken(ConsumerKey, ConsumerSecret);

            var aggregateFeed = new AggregateFeed();
            var concatenatedFeeds = new List<FeedItem>();
            foreach (var twitterHandle in twitterHandles)
            {
                var feedItems = TwitterCorrespondent.GetTimeline(twitterHandle, bearerToken);
                var totalTweets = 0;
                var totalMentions = 0;
                foreach (var feedItem in feedItems)
                {
                    // filter results by datetime
                    if (feedItem.PostedDateTime >= fromDateTime)
                    {
                        totalTweets++;
                        totalMentions += CountMentions(twitterHandle, feedItem.Text);
                        
                        concatenatedFeeds.Add(feedItem);
                        
                    }
                }
                aggregateFeed.MetaData.Add(twitterHandle, new FeedMetaData
                {
                    TotalFeedItems = totalTweets,
                    TotalExternalReferences = totalMentions
                });
            }
            
            aggregateFeed.FeedItems = concatenatedFeeds.OrderByDescending(i => i.PostedDateTime).ToList();

            // NOTE: I spent some time trying to optimize the time complexity of this method.
            // I realize we are looping through and retrieving each feed, then looping through
            // each item in the feed, and adding them to a concatenated list. Adding to the 
            // list is O(1) for each feed item, so O(n), and then I do an order by at the end.
            // I am not sure what algorithm is used in the OrderBy (O(log n) perhaps?), but I tried to optimize
            // this by making the concatenated list a SortedList, and a Heap. These two
            // data structures would sort on insert, rather than sorting the list at the end.
            // However, I ran the tests with different numbers of feedItems (200, 100, 1000, 2500)
            // and I found that the built in OrderBy works best.

            return aggregateFeed;
        }

        private int CountMentions(string twitterHandle, string text)
        {
            /*
             * A wise man once said:
             * Some people, when confronted with a problem, think "I know, I'll use regular expressions." Now they have two problems.
             */
            const string twitterHandlePattern = @"@[A-Za-z0-9_]{1,15}";
            var matches = new Regex(twitterHandlePattern).Matches(text);
            return matches.Cast<object>().Count(match => match.ToString() != twitterHandle);
        }
    }

    #region attempted to optimize with a heap. I would delete this region before going to prod.

    //var concatenatedFeeds = new SortedList<long, FeedItem>();
    //var concatenatedFeeds = new Heap<FeedItem>(new List<FeedItem>(), 0, new FeedItemComparer());

    /*long key = feedItem.PostedDateTime.Ticks;
                        // to prevent collisions.
                        while (concatenatedFeeds.ContainsKey(key))
                        {
                            key++;
                        }*/
    //concatenatedFeeds.Add(key, feedItem); // sorted list
    //concatenatedFeeds.Insert(feedItem); // heap

    //aggregateFeed.FeedItems = concatenatedFeeds.Values;

    //            var count = concatenatedFeeds.Count;
    //            while(count > 0)
    //            {
    //                count--;
    //                aggregateFeed.FeedItems.Add(concatenatedFeeds.PopRoot());
    //            }

    /*public class FeedItemComparer : IComparer<FeedItem>
    {
        public int Compare(FeedItem x, FeedItem y)
        {
            if (x.PostedDateTime.Ticks < y.PostedDateTime.Ticks)
                return 1;

            return -1;
        }
    }*/


    /*
     * Thought I would try to optimize this.
     * Tried using a heap to do the merge of the lists, but
     * It looks like the sort that is available in c# by default does a better job.
     * 
     *  Count	Heap	SortedList	List
        1000	33	    31	        30
        2500	81	    78	        75
        5000	155	    178	        140

     * 
     * 
        public class Heap<T>
        {
            private readonly IList<T> list;
            private readonly IComparer<T> comparer;

            public Heap(IList<T> list, int count, IComparer<T> comparer)
            {
                this.comparer = comparer;
                this.list = list;
                Count = count;
                Heapify();
            }

            public int Count { get; private set; }

            public T PopRoot()
            {
                if (Count == 0) throw new InvalidOperationException("Empty heap.");
                var root = list[0];
                SwapCells(0, Count - 1);
                Count--;
                HeapDown(0);
                return root;
            }

            public T PeekRoot()
            {
                if (Count == 0) throw new InvalidOperationException("Empty heap.");
                return list[0];
            }

            public void Insert(T e)
            {
                if (Count >= list.Count) list.Add(e);
                else list[Count] = e;
                Count++;
                HeapUp(Count - 1);
            }

            private void Heapify()
            {
                for (int i = Parent(Count - 1); i >= 0; i--)
                {
                    HeapDown(i);
                }
            }

            private void HeapUp(int i)
            {
                T elt = list[i];
                while (true)
                {
                    int parent = Parent(i);
                    if (parent < 0 || comparer.Compare(list[parent], elt) > 0) break;
                    SwapCells(i, parent);
                    i = parent;
                }
            }

            private void HeapDown(int i)
            {
                while (true)
                {
                    int lchild = LeftChild(i);
                    if (lchild < 0) break;
                    int rchild = RightChild(i);

                    int child = rchild < 0
                      ? lchild
                      : comparer.Compare(list[lchild], list[rchild]) > 0 ? lchild : rchild;

                    if (comparer.Compare(list[child], list[i]) < 0) break;
                    SwapCells(i, child);
                    i = child;
                }
            }

            private int Parent(int i) { return i <= 0 ? -1 : SafeIndex((i - 1) / 2); }

            private int RightChild(int i) { return SafeIndex(2 * i + 2); }

            private int LeftChild(int i) { return SafeIndex(2 * i + 1); }

            private int SafeIndex(int i) { return i < Count ? i : -1; }

            private void SwapCells(int i, int j)
            {
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }*/

    #endregion
}