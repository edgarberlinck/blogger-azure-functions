
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
    public static class Blog_Post
    {
        [FunctionName("Blog_Post")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Fetching Posts");
            var blogId = req.Query["blogId"];
            var postId = req.Query["postId"];

            var data = GetPost(blogId, postId);
            return new OkObjectResult(data);
        }

        private static ExpandoObject GetPost(string BlogId, string PostId)
        {
            try
            {

                var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);
                var url = $"https://www.googleapis.com/blogger/v3/blogs/{BlogId}/posts/{PostId}?key={key}";

                dynamic data = Client.Call(url);
                dynamic post = new ExpandoObject();

                post.id = data.id;
                post.title = data.title;
                post.author = data.author;
                post.commentCount = data.replies.totalItems;
                post.labels = data.labels;
                post.content = data.content;
                post.published = data.published;

                return post;
            }
            catch
            {
                return null;
            }
        }
    }
}
