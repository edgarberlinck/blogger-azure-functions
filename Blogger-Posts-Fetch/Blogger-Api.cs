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
using System.Collections.Generic;

namespace BloggeApi
{
    public static class Blogger_Api
    {
        [FunctionName("BlogInfo")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("Fetching blog Info");
            var data = GetBlogBasicInfo();

            return (data == null) ? new BadRequestObjectResult("Se fudeu!") : (ActionResult)new OkObjectResult(data);
        }

        [FunctionName("BlogPosts")]
        public static IActionResult BlogPosts([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("Fetching Blog Posts");

            if (!req.GetQueryParameterDictionary().TryGetValue("blogId", out string blogId))
            {
                log.Error("blogId parameter missing");
                return new BadRequestObjectResult("blogId parameter missing");
            }

            var data = FetchPosts(blogId);
            return new OkObjectResult(data);
        }

        [FunctionName("BlogPost")]
        public static IActionResult BlogPost([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)]HttpRequest req, TraceWriter log)
        {
            log.Info("Fetching a Blog Posts");

            if (!req.GetQueryParameterDictionary().TryGetValue("blogId", out string blogId))
            {
                log.Error("blogId parameter missing");
                return new BadRequestObjectResult("blogId parameter missing");
            }

            if (!req.GetQueryParameterDictionary().TryGetValue("postId", out string postId))
            {
                log.Error("postId parameter missing");
                return new BadRequestObjectResult("postId parameter missing");
            }

            var data = GetPost(blogId, postId);
            return new OkObjectResult(data);
        }

        //-------------------------
        // Helper functions
        //-------------------------
        private static ExpandoObject GetBlogBasicInfo() {
            try
            {
                var blogUri = Environment.GetEnvironmentVariable("BLOGGER_URI", EnvironmentVariableTarget.Process);
                var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);

                var url = $"https://www.googleapis.com/blogger/v3/blogs/byurl?url={blogUri}&key={key}";

                dynamic data = Call(url);
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

        private static IEnumerable<ExpandoObject> FetchPosts (string BlogId) {
            var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);
            
            var url = $"https://www.googleapis.com/blogger/v3/blogs/{BlogId}/posts?key={key}&fetchBodies=false";
            
            dynamic data = Call(url);

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

        private static ExpandoObject GetPost(string BlogId, string PostId)
        {
            try
            {
               
                var key = Environment.GetEnvironmentVariable("BLOGGER_API_KEY", EnvironmentVariableTarget.Process);
                var url = $"https://www.googleapis.com/blogger/v3/blogs/{BlogId}/posts/{PostId}?key={key}";

                dynamic data = Call(url);
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

        private static ExpandoObject Call(string Url)
        {
            var request = (HttpWebRequest)WebRequest.Create(Url);
            request.ContentType = "application/json; charset=utf-8";

            var response = request.GetResponse() as HttpWebResponse;
            using (var responseStream = response.GetResponseStream())
            {
                var streamReader = new StreamReader(responseStream);
                return JsonConvert.DeserializeObject<ExpandoObject>(streamReader.ReadToEnd());
            }
        }
    }
}
