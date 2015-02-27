using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using pbpTwitterExercise.Models.Correspondents;
using pbpTwitterExercise.Models.Services;

namespace pbpTwitterExercise.Controllers
{
    /*
     * NOTE: a controller that inherits from apiController
     * would also be perfect for this use case, but I am going to KISS (keep it simple, stupid).
     * 
     */

    public class FacebookController : Controller
    {
        public FacebookCorrespondent fb = new FacebookCorrespondent();

        //
        // GET: /Twitter/

        public ActionResult Index()
        {
            var allResults = fb.GetMentions();
            var compiledResults = new Dictionary<string, FbAggregate>();

            foreach (var user in allResults.Comments)
            {
                if (!compiledResults.ContainsKey(user.Id))
                {
                    compiledResults.Add(user.Id, new FbAggregate
                    {
                        Name = user.Name,
                        TotalComments = 0,
                        TotalLikes = 0
                    });
                }

                var u = compiledResults[user.Id];
                u.TotalComments++;
            }

            foreach (var user in allResults.Likes)
            {
                if (!compiledResults.ContainsKey(user.Id))
                {
                    compiledResults.Add(user.Id, new FbAggregate
                    {
                        Name = user.Name,
                        TotalComments = 0,
                        TotalLikes = 0
                    });
                }

                var u = compiledResults[user.Id];
                u.TotalLikes++;
            }

            return View(compiledResults);
        }

        public ActionResult Posts()
        {
            var allResults = fb.GetPopularPosts();
            var compiledResults = new Dictionary<string, FbAggregate>();

            foreach (var post in allResults["likes"])
            {
                if (!compiledResults.ContainsKey(post.Id))
                {
                    compiledResults.Add(post.Id, new FbAggregate
                    {
                        TotalComments = 0,
                        TotalLikes = 0
                    });
                }

                var u = compiledResults[post.Id];
                u.TotalLikes += post.Count;
            }

            foreach (var post in allResults["comments"])
            {
                if (!compiledResults.ContainsKey(post.Id))
                {
                    compiledResults.Add(post.Id, new FbAggregate
                    {
                        TotalComments = 0,
                        TotalLikes = 0
                    });
                }

                var u = compiledResults[post.Id];
                u.TotalComments += post.Count;
            }

            return View(compiledResults);
        }
    }

    public class FbAggregate
    {
        public string Name { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
    }
}
