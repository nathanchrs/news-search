using news_search.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace news_search.Libs
{
    public class RssParser
    {
        public static async Task<IEnumerable<Post>> ReadFeedAsync(string url)
        {
            HttpClient client = new HttpClient();
            var response = await client.GetStreamAsync(url);
            var rssFeed = XDocument.Load(response);

            var posts = from item in rssFeed.Descendants("item")
                        select new Post
                        {
                            Title = item.Element("title").Value,
                            Link = item.Element("link").Value,
                            Description = item.Element("description").Value,
                            PublishedDate = item.Element("pubDate").Value
                        };

            return posts;
        }
    }
}
