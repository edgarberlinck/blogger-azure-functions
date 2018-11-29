using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net;
using System;
using System.Dynamic;

namespace BloggerPostsFetch
{
    public static class Blogger_Posts_Fetch
    {
        private static string GetPostsUri() {
            try
            {
                var blogUri = Environment.GetEnvironmentVariable("BLOGGER_URI", EnvironmentVariableTarget.Process);
                var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);

                var url = $"https://www.googleapis.com/blogger/v3/blogs/byurl?url={blogUri}&key={key}";
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json; charset=utf-8";

                var response = request.GetResponse() as HttpWebResponse;
                using (var responseStream = response.GetResponseStream())
                {
                    var streamReader = new StreamReader(responseStream);
                    dynamic json = JsonConvert.DeserializeObject<ExpandoObject>(streamReader.ReadToEnd());

                    return json.posts.selfLink;
                }
            }
            catch
            {
                return null;
            }
        }

        private static object FetchPosts (string uri) {
            try
            {
                var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);
                string url = $"{uri}?key={key}";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json; charset=utf-8";

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader streamReader = new StreamReader(responseStream);
                    var json = JsonConvert.DeserializeObject(streamReader.ReadToEnd());

                    return json;
                }
            }
            catch
            {
                return null;
            }
        } 

        [FunctionName("Blogger_Posts_Fetch")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            var uri = GetPostsUri();

            return (uri == null)? new BadRequestObjectResult("Se fudeu!") : (ActionResult)new OkObjectResult(FetchPosts(uri));

        }
    }
}
