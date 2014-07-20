using System.Web.Mvc;
using Microsoft.Practices.Unity;
using pbpTwitterExercise.Models.Correspondents;
using pbpTwitterExercise.Models.Services;
using Unity.Mvc4;

namespace pbpTwitterExercise
{
    public static class Bootstrapper
    {
        public static IUnityContainer Initialise()
        {
            var container = BuildUnityContainer();

            DependencyResolver.SetResolver(new UnityDependencyResolver(container));

            return container;
        }

        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // naming these "twitter", because we may very well have services, and correspondents
            // for other services in the future. Such as faceook or instagram.
            container.RegisterType<ISocialCorrespondent, TwitterCorrespondent>("twitter");
            container.RegisterType<ISocialService, TwitterService>("twitter");
            RegisterTypes(container);

            return container;
        }

        public static void RegisterTypes(IUnityContainer container)
        {

        }
    }
}