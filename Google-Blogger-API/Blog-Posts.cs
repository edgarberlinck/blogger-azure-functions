
using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Dynamic;
using System.Net;
using Google_Blogger_API;

namespace BloggerApi
{
    public static class Blog_Posts
    {
        [FunctionName("Blog_Posts")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]HttpRequest req, ILogger log)
        {
            log.LogInformation("Fetching Blog Posts");

            string blogId = req.Query["blogId"];
            log.LogInformation($"Received blogId = {blogId}");
            var data = FetchPosts(blogId);
            return new OkObjectResult(data);
        }

        private static IEnumerable<ExpandoObject> FetchPosts (string BlogId) {
            var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);

            var url = $"https://www.googleapis.com/blogger/v3/blogs/{BlogId}/posts?key={key}&fetchBodies=false";

            dynamic data = Client.Call(url);

            foreach (var item in data.items)
            {
                dynamic post = new ExpandoObject();
                post.id = item.id;
                post.title = item.title;
                post.author = item.author;
                post.commentCount = item.replies.totalItems;
                post.labels = item.labels;
                post.published = item.published;

                yield return post;
            }
        }
    }
}
