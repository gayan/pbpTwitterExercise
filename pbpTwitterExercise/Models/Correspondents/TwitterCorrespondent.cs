using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using pbpTwitterExercise.Models.Exceptions;

namespace pbpTwitterExercise.Models.Correspondents
{
    public class TwitterCorrespondent : ISocialCorrespondent
    {
        private readonly static string ApiBaseUrl = WebConfigurationManager.AppSettings["Twitter.ApiBaseUrl"];
        private const int MaxTweets = 200;

        public string GetBearerToken(string consumerKey, string consumerSecret)
        {   
            // Check for errors first, and return early.
            if (string.IsNullOrEmpty(consumerKey) || string.IsNullOrEmpty(consumerSecret))
                return null;

            // Following https://dev.twitter.com/docs/auth/application-only-auth
            // to do Application-only authentication, and return the bearer token.
            
            // Step 1: Encode consumer key and secret
            var encodedConsumerKey = HttpUtility.UrlEncode(consumerKey, Encoding.UTF8); // CAUTION: is UrlEncode RFC 1738 compliant?
            var encodedConsumerSecret = HttpUtility.UrlEncode(consumerSecret, Encoding.UTF8); // CAUTION: is UrlEncode RFC 1738 compliant?
            var bearerTokenCredentials = encodedConsumerKey + ":" + encodedConsumerSecret;

            var base64EncodedBearerTokenCredentials =
                Convert.ToBase64String(Encoding.UTF8.GetBytes(bearerTokenCredentials));

            // Step 2: Obtain a bearer token
            TwitterAccessToken bearerToken;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
                    base64EncodedBearerTokenCredentials);

                var oauthTokenEndPoint = WebConfigurationManager.AppSettings["Twitter.OAuthTokenEndpoint"];
                var request = new HttpRequestMessage(HttpMethod.Post, oauthTokenEndPoint);
                
                // NOTE: Hard coded values. No big win in using constants or putting these values in config at this time. Some would disagree. It's worth a debate.
                request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

                var response = client.SendAsync(request).Result;
                response.EnsureSuccessStatusCode(); // throw an exception if twitter request failed.
                bearerToken = response.Content.ReadAsAsync<TwitterAccessToken>().Result;
            }

            return bearerToken != null ? bearerToken.AccessToken : null;
        }

        public IList<FeedItem> GetTimeline(string user, string bearerToken)
        {
            // early returns for errors
            if (string.IsNullOrEmpty(user))
                return null;

            if(bearerToken == null)
                throw new AuthenticationException("No bearer token provided");

            // Calling as described in doc https://dev.twitter.com/docs/api/1.1/get/statuses/user_timeline
            // to get a user's timeline.

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(ApiBaseUrl);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);

                // twitter doesn't provide much filtering capability by datetime. So we will get the max tweets,
                // and then filter them as required at a higher layer of the application.
                var relativeUrl = string.Format("{0}?{1}={2}&count={3}",
                    WebConfigurationManager.AppSettings["Twitter.UserTimelineEndpoint"], "screen_name", user, MaxTweets);

                // NOTE: It may be preffered to use the uri builder to add query string params, rather than the string.format
                // code above. In this simple scenario, I think the above code works just fine. For a more complex situation
                // we might do something as follows:
                //                var uriBuilder = new UriBuilder(ApiBaseUrl);
                //                var queryString = HttpUtility.ParseQueryString(uriBuilder.Query);
                //                queryString["screen_name"] = user;
                //                queryString["count"] = MaxTweets.ToString();
                

                var result = client.GetAsync(relativeUrl).Result;
                if (result.IsSuccessStatusCode)
                {
                    var twitterItems = result.Content.ReadAsAsync<IList<TwitterFeedItem>>().Result;
                    return (from t in twitterItems select t.ToFeedItem(user)).ToList();
                }
                
                if(result.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SocialUserNotFoundException(user);
                }

                result.EnsureSuccessStatusCode(); // throw exception, call to twitter was not successful.

                return null;
            }
        }
    }

    public class TwitterDateTimeConverter : DateTimeConverterBase
    {
        const string TwitterDateTimeFormat = "ddd MMM dd HH:mm:ss zzzz yyyy"; // hard coded because this converter is very specific for twitter. One might argue otherwise.
        
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(((DateTime)value).ToString(TwitterDateTimeFormat));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return DateTime.ParseExact(reader.Value.ToString(), TwitterDateTimeFormat, CultureInfo.InvariantCulture);
        }
    }

    #region data transfer objects (DTO)

    [JsonObject]
    public class TwitterFeedItem
    {
        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "created_at")]
        [JsonConverter(typeof(TwitterDateTimeConverter))]
        public DateTime CreatedAt { get; set; }

        public FeedItem ToFeedItem(string accountName)
        {
            return new FeedItem
            {
                AccountName = accountName,
                PostedDateTime = CreatedAt,
                Text = Text
            };
        }
    }
    
    [JsonObject]
    public class TwitterAccessToken
    {
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; }

        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; }
    }

    #endregion
}