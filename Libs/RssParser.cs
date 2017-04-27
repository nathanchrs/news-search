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
        public static readonly Dictionary<string, string> rssSources = new Dictionary<string, string> {
            {"Detik", @"http://rss.detik.com/"},
            {"Viva News", @"http://rss.viva.co.id/get/all"}
        };

        // Load posts by querying the specified RSS fields.
        public static async Task<IEnumerable<Post>> ReadFeedsAsync()
        {
            var posts = new List<Post>();
            foreach (var rssSourceName in rssSources.Keys) {
                try
                {
                    HttpClient client = new HttpClient();
                    var response = await client.GetStreamAsync(rssSources[rssSourceName]);
                    var rssFeed = ReadXML(response);
                    var newPosts = from item in rssFeed.Descendants("item")
                                    select new Post
                                    {
                                        Title = item.Element("title").Value,
                                        Link = item.Element("link").Value,
                                        Description = item.Element("description").Value,
                                        PublishedDate = item.Element("pubDate").Value,
                                        RssSource = rssSourceName
                                    };
                    posts.AddRange(newPosts);                    
                }
                catch (System.Exception) {
                    System.Console.WriteLine("[WARNING] Failed to read RSS XML from " + rssSourceName);
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
