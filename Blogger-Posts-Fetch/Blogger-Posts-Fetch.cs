using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System.Net;
using Newtonsoft.Json.Linq;
using System;

namespace BloggerPostsFetch
{
    public static class Blogger_Posts_Fetch
    {
        private static string getPostsUri() {
            try
            {
                var blogUri = Environment.GetEnvironmentVariable("BLOGGER_URI", EnvironmentVariableTarget.Process);
                var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);

                string url = $"https://www.googleapis.com/blogger/v3/blogs/byurl?url={blogUri}&key={key}";
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.ContentType = "application/json; charset=utf-8";

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader streamReader = new StreamReader(responseStream);
                    var json = JsonConvert.DeserializeObject(streamReader.ReadToEnd());
                    JObject obj = JObject.Parse(json.ToString());

                    return obj["posts"]["selfLink"].ToString();
                }
            }
            catch
            {
                return null;
            }
        }

        private static object fetchPosts (string uri) {
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

            var uri = getPostsUri();

            return (uri == null)? new BadRequestObjectResult("Se fudeu!") : (ActionResult)new OkObjectResult(fetchPosts(uri));

        }
    }
}
