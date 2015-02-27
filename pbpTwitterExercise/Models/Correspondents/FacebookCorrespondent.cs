using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;

namespace pbpTwitterExercise.Models.Correspondents
{
    public class FacebookCorrespondent
    {
        private string startingpPoint = "https://graph.facebook.com/v2.2/sasktel/posts?access_token=CAACEdEose0cBAHfrZC0ZAt62j6yyoqn4wgqOAZCi5EaAJYMC4T2d2quM8bYennOZBxxfjZBOqEzsiiEw7PhV4udwHzdEBa4qnFdx5TmognwwpcbqV1cpa49ZCbImqZBcNkvq8RFZARqhHZCdPkocziIKZB4FqncKIVkBgn9ow7FNXPHB0YMaOS0MYGR0egCbq9qltRnU1CHLf73XE3ZC1IyWi2tjmlXidWyH0cZD";

        public FbEngagement GetMentions()
        {
            var likes = GetLikes(startingpPoint);
            var comments = GetComments(startingpPoint);

            return new FbEngagement
            {
                Likes = likes,
                Comments = comments
            };
        }

        private IList<FbUser> GetComments(string endpoint)
        {
            var comments = new List<FbUser>();

            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);
                foreach (var post in dynObj.data)
                {
                    if (post.comments != null)
                    {
                        foreach (var comment in post.comments.data)
                        {
                            comments.Add(new FbUser
                            {
                                Id = comment.from.id,
                                Name = comment.from.name
                            });
                        }

                        // and then we must page through likes.
                        try
                        {
                            string moreComments = post.comments.paging.next;
                            comments.AddRange(GetMoreComments(moreComments));
                        }
                        catch
                        {
                            // no paging to be done.
                        }
                    }
                }

                try
                {
                    string nextPage = dynObj.paging.next;
                    comments.AddRange(GetComments(nextPage));
                }
                catch
                {
                    // no paging to be done.
                }
            }

            return comments;
        }

        private IEnumerable<FbUser> GetMoreComments(string endpoint)
        {
            var comments = new List<FbUser>();

            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);

                foreach (var comment in dynObj.data)
                {
                    comments.Add(new FbUser
                    {
                        Id = comment.from.id,
                        Name = comment.from.name
                    });
                }

                // and then we must page through likes.
                try
                {
                    string moreComments = dynObj.paging.next;
                    comments.AddRange(
                        GetMoreComments(moreComments));
                }
                catch
                {
                    // no paging to be done.
                }
            }

            return comments;
        }

        private IList<FbUser> GetLikes(string endpoint)
        {
            var likes = new List<FbUser>();


            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);
                foreach (var post in dynObj.data)
                {
                    foreach (var like in post.likes.data)
                    {
                        likes.Add(new FbUser
                        {
                            Id = like.id,
                            Name = like.name
                        });
                    }

                    string moreLikes;
                    // and then we must page through likes.
                    try
                    {
                        moreLikes = post.likes.paging.next;
                    }
                    catch
                    {
                        moreLikes = null;
                    }

                    if (moreLikes != null)
                    {
                        likes.AddRange(GetMoreLikes(moreLikes));    
                    }
                    
                }

                try
                {
                    string nextPage = dynObj.paging.next;
                    likes.AddRange(GetLikes(nextPage));
                }
                catch
                {
                    // no paging to be done.
                }
            }

            return likes;
        }

        private IEnumerable<FbUser> GetMoreLikes(string endpoint)
        {
            var likes = new List<FbUser>();

            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);

                foreach (var like in dynObj.data)
                {
                    likes.Add(new FbUser
                    {
                        Id = like.id,
                        Name = like.name
                    });
                }

                string moreLikes;
                // and then we must page through likes.
                try
                {
                    moreLikes = dynObj.paging.next;
                }
                catch
                {
                    moreLikes = null;
                }

                if (moreLikes != null)
                {
                    likes.AddRange(GetMoreLikes(moreLikes));   
                }
            }

            return likes;
        }

        public IDictionary<string, IList<PostMetaData>> GetPopularPosts()
        {
            var result = new Dictionary<string, IList<PostMetaData>>();
            result.Add("likes", CountLikes(startingpPoint));
            result.Add("comments", CountComments(startingpPoint));

            return result;
        }

        private IList<PostMetaData> CountComments(string endpoint)
        {
            var comments = new List<PostMetaData>();

            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);
                foreach (var post in dynObj.data)
                {
                    if (post.comments != null)
                    {
                        IEnumerable<Object> a = post.comments.data;
                        var count = a.Count();
                        
                        // and then we must page through likes.
                        try
                        {
                            string moreComments = post.comments.paging.next;
                            count += CountMoreComments(moreComments);
                        }
                        catch
                        {
                            // no paging to be done.
                        }

                        comments.Add(new PostMetaData
                        {
                            Id = post.id,
                            Count = count
                        });
                    }
                }

                try
                {
                    string nextPage = dynObj.paging.next;
                    comments.AddRange(CountComments(nextPage));
                }
                catch
                {
                    // no paging to be done.
                }
            }

            return comments;
        }

        private int CountMoreComments(string endpoint)
        {
            int count;

            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);

                IEnumerable<Object> a = dynObj.data;
                count = a.Count();
                
                // and then we must page through likes.
                try
                {
                    string moreComments = dynObj.paging.next;
                    count += CountMoreComments(moreComments);
                }
                catch
                {
                    // no paging to be done.
                }
            }

            return count;
        }
        
        private IList<PostMetaData> CountLikes(string endpoint)
        {
            var likes = new List<PostMetaData>();

            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);
                foreach (var post in dynObj.data)
                {
                    IEnumerable<Object> a = post.likes.data;
                    var likeCount = a.Count();

                    string moreLikes;
                    // and then we must page through likes.
                    try
                    {
                        moreLikes = post.likes.paging.next;
                        
                    }
                    catch
                    {
                        moreLikes = null;
                    }

                    if (moreLikes != null)
                    {
                        likeCount += CountMoreLikes(moreLikes);    
                    }

                    likes.Add(new PostMetaData
                    {
                        Id = post.id,
                        Count = likeCount
                    });
                }

                try
                {
                    string nextPage = dynObj.paging.next;
                    likes.AddRange(CountLikes(nextPage));
                }
                catch
                {
                    // no paging to be done.
                }
            }

            return likes;
        }

        private int CountMoreLikes(string endpoint)
        {
            int likes;

            using (var client = new HttpClient())
            {
                var jsonString = client.GetStringAsync(endpoint).Result;
                dynamic dynObj = JsonConvert.DeserializeObject(jsonString);

                IEnumerable<Object> a = dynObj.data;
                likes = a.Count();

                string moreLikes;
                // and then we must page through likes.
                try
                {
                    moreLikes = dynObj.paging.next;
                }
                catch
                {
                    moreLikes = null;
                }

                if (moreLikes != null)
                {
                    likes += CountMoreLikes(moreLikes);    
                }
            }

            return likes;
        }
    }

    public class FbEngagement
    {
        public IList<FbUser> Likes;
        public IList<FbUser> Comments;
    }

    public class PostMetaData
    {
        public string Id { get; set; }
        public int Count { get; set; }
    }

    public class FbUser
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}