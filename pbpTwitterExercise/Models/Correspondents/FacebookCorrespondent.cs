using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using Newtonsoft.Json;

namespace pbpTwitterExercise.Models.Correspondents
{
    public class FacebookCorrespondent
    {
        public FbEngagement GetMentions()
        {
            var endpoint = "https://graph.facebook.com/v2.2/SaskTel/posts?access_token=CAACEdEose0cBAJ50AZA2YGeazTIwmqRP1UZBkBSx2Oh4XHX2EtreGZBiubY6l8ZA176xwjHxWWiLgZBHrZC3bZAWXl7cYBjfLk5hE6Ha7CKWdiDCEf5QscBqSJYWgIRvpQ6uc4PbQN07i8tL768bak2w5C3OpGYQ3v1BMj93xrn9WxrApU6BDHRzcxCelJBagmXEgwJDfyGNpo4ykBSocOl";
            var likes = GetLikes(endpoint);
            var comments = GetComments(endpoint);

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

                    // and then we must page through likes.
                    try
                    {
                        var moreLikes = post.comments.paging.next;
                        likes.AddRange(
                            GetMoreLikes(moreLikes));
                    }
                    catch
                    {
                        // no paging to be done.
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

                // and then we must page through likes.
                try
                {
                    string moreLikes = dynObj.paging.next;
                    likes.AddRange(
                        GetMoreLikes(moreLikes));
                }
                catch
                {
                    // no paging to be done.
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

    public class FbUser
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }
}