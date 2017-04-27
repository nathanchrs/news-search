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
            } catch (Exception e) {
                Console.WriteLine("[DEBUG] Failed to fetch HTML for URL " + post.Link);
            }
            
            try {
                ParseHTML(post, document);
            } catch (Exception) {
                Console.WriteLine("[DEBUG] Failed to parse HTML for URL " + post.Link);
            }
        }

        private static void ParseHTML(Post post, HtmlDocument document) {
            // DEBUG
            post.Content = "huehuehue";
        }

        private static void RemoveComments(HtmlNode node)
        {
            foreach (var n in node.ChildNodes.ToArray())
                RemoveComments(n);
            if (node.NodeType == HtmlNodeType.Comment)
                node.Remove();
        }
        
        public static String ParsingDetik(HtmlDocument document, String type = "")
        {
            var root = document.DocumentNode;

            RemoveComments(root);

            root.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "iframe" || n.Name == "span" || n.Name == "br")
                .ToList()
                .ForEach(n => n.Remove());

            if (type.Equals("title"))
            {
                var title = root.Descendants("div")
                    .Where(n => n.GetAttributeValue("class", "").Equals("jdl"))
                    .Single()
                    .Descendants("h1")
                    .Single();
                return title.InnerText;
            }
            else if (type.Equals("date"))
            {
                var date = root.Descendants("div")
                    .Where(n => n.GetAttributeValue("class", "jdl").Equals("date"))
                    .Single();
                return date.InnerText;
            }
            else if (type.Equals("location"))
            {
                var location = root.Descendants()
                    .Where(n => n.GetAttributeValue("class", "").Equals("detail_text"))
                    .Single()
                    .Descendants("b")
                    .Single();
                return location.InnerText;
            }
            else
            {
                var paragraph = root.Descendants()
                    .Where(n => n.GetAttributeValue("class", "").Equals("detail_text"))
                    .Single();

                String content = Regex.Replace(paragraph.InnerText, @"\s+", " ");
                return content.Trim();
            }
        }


        public static String ParsingTempo(HtmlDocument node, String type = "")
        {
            var root = node.DocumentNode;

            /**
            root.Descendants()
                .Where(n => n.Name == "script" || n.Name == "style" || n.Name == "iframe" || n.Name == "span" || n.Name == "br" || n.Name == "ul")
                .ToList()
                .ForEach(n => n.Remove());

            var date = root.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "jdl").Equals("date"))
                .Single();

            var title = root.Descendants("div")
                .Where(n => n.GetAttributeValue("class", "").Equals("jdl"))
                .Single()
                .Descendants("h1")
                .Single();

            var location = root.Descendants()
                .Where(n => n.GetAttributeValue("class", "").Equals("detail_text"))
                .Single()
                .Descendants("b")
                .Single();
            **/

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
            return content;
            //Console.WriteLine(date.InnerText);
            // Console.WriteLine(judul.InnerText);
            //Console.WriteLine(location.InnerText);
            //Console.WriteLine(content);

        }
    }
}
