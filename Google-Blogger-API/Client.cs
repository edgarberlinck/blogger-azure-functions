using System;
using System.Dynamic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace Google_Blogger_API
{
    public class Client
    {
        public static ExpandoObject Call(string Url)
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
