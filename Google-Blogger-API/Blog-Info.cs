
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Dynamic;
using System.Net;
using Google_Blogger_API;

namespace BloggerApi
{
    public static class Blog_Info
    {
        [FunctionName("Blog_Info")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Fetching Blog Info");
            var data = GetBlogBasicInfo();

            return (data == null) ? new BadRequestObjectResult("Se fudeu!") : (ActionResult)new OkObjectResult(data);
        }

        private static ExpandoObject GetBlogBasicInfo()
        {
            try
            {
                var blogUri = Environment.GetEnvironmentVariable("BLOGGER_URI", EnvironmentVariableTarget.Process);
                var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);

                var url = $"https://www.googleapis.com/blogger/v3/blogs/byurl?url={blogUri}&key={key}";

                dynamic data = Client.Call(url);
                dynamic blog = new ExpandoObject();

                blog.description = data.description;
                blog.id = data.id;
                blog.title = data.name;
                blog.numberOfPosts = data.posts.totalItems;

                return blog;
            }
            catch
            {
                return null;
            }
        }
    }
}
