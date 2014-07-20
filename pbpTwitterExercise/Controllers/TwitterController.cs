using System;
using System.Net;
using System.Web.Mvc;
using Microsoft.Practices.Unity;
using pbpTwitterExercise.Models.Services;

namespace pbpTwitterExercise.Controllers
{
    /*
     * NOTE: a controller that inherits from apiController
     * would also be perfect for this use case, but I am going to KISS (keep it simple, stupid).
     * 
     */

    public class TwitterController : Controller
    {
        [Dependency("twitter")]
        public ISocialService TwitterService { get; set; }

        //
        // GET: /Twitter/

        public ActionResult Index()
        {
            try
            {
                return Json(TwitterService.GetAggregateFeed(new[] { "@pay_by_phone", "@PayByPhone", "@PayByPhone_UK" },
                    DateTime.Now.AddDays(-14)), JsonRequestBehavior.AllowGet); // -14 => two weeks ago.
            }
            // NOTE: If those twitter accounts were not hard coded above, and were parameters, this catch clause would be of use. 
            //catch (SocialUserNotFoundException e)
            //{
            //  return new HttpStatusCodeResult(HttpStatusCode.NotFound, e.Message);
            //}
            catch (Exception e)
            {
                // Let the client know something has gone wrong, with the correct HTTP status code.
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, e.Message);
            }
        }

    }
}
