using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using news_search.Models;

namespace news_search.Libs
{
    public class HtmlParser
    {  
        // Fetch and parse content in parallel for a list of posts; fill their content values.
        public static async Task FetchPostContents(IEnumerable<Post> posts) {
            // Fetch HTML documents for each post in parallel
            var fetchTasks = new Task[posts.Count()];
            int i = 0;
            foreach (var post in posts) {
                fetchTasks[i] = FetchPostContent(post);
                i++;
            }
            await Task.WhenAll(fetchTasks);
        }

        // Fetch and parse content of a post; fill its content value.
        private static async Task FetchPostContent(Post post)
        {
            HtmlDocument document = new HtmlDocument();
            try {
                
                using (HttpClient client = new HttpClient())
                using (HttpResponseMessage response = await client.GetAsync(post.Link))
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync();
                    document.LoadHtml(result);
                }
            } catch (Exception) {
                Console.WriteLine("[DEBUG] Failed to fetch HTML for URL " + post.Link);
            }
            
            try {
                ParseHtml(post, document);
            } catch (Exception) {
                Console.WriteLine("[DEBUG] Failed to parse HTML for URL " + post.Link);
            }
        }

        private static void ParseHtml(Post post, HtmlDocument document) {
            // Remove comments
            RemoveComments(document.DocumentNode);

            // Parse specific HTML
            if (post.RssSource == "Detik") {
                ParseHtmlDetik(post, document);
            } else if (post.RssSource == "Viva") {
                ParseHtmlViva(post, document);
            } else if (post.RssSource == "Antara") {
                ParseHtmlAntara(post, document);
            } else if (post.RssSource == "Tempo") {
                ParseHtmlTempo(post, document);
            }
        }

        private static void RemoveComments(HtmlNode node)
        {
            foreach (var n in node.ChildNodes.ToArray())
                RemoveComments(n);
            if (node.NodeType == HtmlNodeType.Comment)
                node.Remove();
        }




        public static void ParseHtmlDetik(Post post, HtmlDocument document)
        {
            var root = document.DocumentNode;

            root.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "iframe" || n.Name == "span" || n.Name == "br")
                .ToList()
                .ForEach(n => n.Remove());

            var title = root.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "").Equals("jdl"))
                .Single()
                .Descendants("h1")
                .Single();

            post.Title = title.InnerText;


            var date = root.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "jdl").Equals("date"))
                .Single();

            post.PublishedDate = date.InnerText;

            var location = root.Descendants()
                .Where(n => n.GetAttributeValue("class", "").Equals("detail_text"))
                .Single()
                .Descendants("b")
                .Single();
            post.Location = location.InnerText;


            var paragraph = root.Descendants()
                .Where(n => n.GetAttributeValue("class", "").Equals("detail_text"))
                .Single();

            String content = Regex.Replace(paragraph.InnerText, @"\s+", " ");
            post.Content = content.Trim();

        }


        public static void ParseHtmlTempo(Post post, HtmlDocument node)
        {
            var root = node.DocumentNode;

            var paragraph = root.SelectNodes("//div[@class='artikel']/p/text()");

            var par2 = root.Descendants()
                .Where(n => n.GetAttributeValue("class", "").Equals("artikel"))
                .Single()
                .Descendants("p")
                .ToList();

            String content = "";

            foreach (var par in par2)
            {
                par.Descendants()
                    .Where(n => n.Name == "a" || n.Name == "em")
                    .ToList()
                    .ForEach(n => n.Remove());

                
                par.Descendants("em")
                    .Where(n => n.Name == "strong")
                    .ToList()
                    .ForEach(n => n.Remove());
                
                content += HtmlEntity.DeEntitize(par.InnerText);
            }

            content = Regex.Replace(content, @"\s+", " ");
            var location = content.Split(' ')[1];
            post.Location = location;
            post.Content = content;
        }

        public static void ParseHtmlViva(Post post, HtmlDocument node)
        {
            var root = node.DocumentNode;
            
            var par2 = root.Descendants()
                .Where(n => n.GetAttributeValue("itemprop", "").Equals("description"))
                .Single()
                .Descendants("p")
                .ToList();

            String content = "";

            foreach (var par in par2)
            {
                par.Descendants()
                    .Where(n => n.Name == "a")
                    .ToList()
                    .ForEach(n => n.Remove());
                content += HtmlEntity.DeEntitize(par.InnerText);
            }

            content = Regex.Replace(content, @"\s+", " ");
            post.Content = content;
        }

        public static void ParseHtmlAntara(Post post, HtmlDocument node)
        {
            var root = node.DocumentNode;

            var par2 = root.Descendants()
                .Where(n => n.GetAttributeValue("itemprop", "").Equals("articleBody"))
                .Single()
                .Descendants("div")
                .ToList();

            String content = "";

            foreach (var par in par2)
            {
                par.Descendants()
                    .Where(n => n.Name == "a" ||n.Name == "script")
                    .ToList()
                    .ForEach(n => n.Remove());

                content += HtmlEntity.DeEntitize(par.InnerText);
            }

            content = Regex.Replace(content, @"\s+", " ");
            content.Replace("(adsbygoogle = window.adsbygoogle || []).push({});", "");
            var location = content.IndexOf(" ") > -1
                  ? content.Substring(0, content.IndexOf(" "))
                  : content;
            post.Location = location;
            post.Content = content;
        }
    }
}
