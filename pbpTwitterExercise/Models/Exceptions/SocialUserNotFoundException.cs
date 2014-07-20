using System;
using System.Runtime.Serialization;

namespace pbpTwitterExercise.Models.Exceptions
{
    [Serializable]
    public class SocialUserNotFoundException : Exception
    {
        /*
         * Having this custom exception class will give us the 
         * ability to handle this exception specifically, in the future.
         * Depending on the situation we may want to fail silently
         * or warn the user about it.
         */

        private readonly string invalidUser;

        public SocialUserNotFoundException(string user)
        {
            invalidUser = user;
        }

        public override string Message
        {
            get { return string.Format("User not found: " + invalidUser); }
        }
    }
}