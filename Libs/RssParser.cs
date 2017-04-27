using news_search.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace news_search.Libs
{
    public class RssParser
    {
        // RSS URLs to load content from.
        public static readonly string[] rssUrls = {
            @"http://rss.detik.com/",
            @"http://rss.viva.co.id/get/all"
        };

        // Load posts by querying the specified RSS fields.
        public static async Task<IEnumerable<Post>> ReadFeedsAsync()
        {
            var posts = new List<Post>();
            foreach (var url in rssUrls) {
                try
                {
                    HttpClient client = new HttpClient();
                    var response = await client.GetStreamAsync(url);
                    var rssFeed = ReadXML(response);
                    var newPosts = from item in rssFeed.Descendants("item")
                                    select new Post
                                    {
                                        Title = item.Element("title").Value,
                                        Link = item.Element("link").Value,
                                        ImageLink = item.Element("enclosure").Attribute("url").Value,
                                        Description = item.Element("description").Value,
                                        PublishedDate = item.Element("pubDate").Value
                                    };
                    posts.AddRange(newPosts);                    
                }
                catch (System.Exception) {
                    System.Console.WriteLine("[WARNING] Failed to read RSS XML from URL " + url);
                }
            }
            return posts;
        }

        // Reads XML without checking for invalid character(s).
        private static XDocument ReadXML(Stream inputStream)
        {
            XDocument document = null;
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings { CheckCharacters = false };
            using (XmlReader xmlReader = XmlReader.Create(inputStream,  xmlReaderSettings))
            {
                xmlReader.MoveToContent();
                document = XDocument.Load(xmlReader);
            }
            return document;
        }
    }
}
