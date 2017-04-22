using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace news_search.Libs
{
    public class HtmlParser
    {
        private static async Task<HtmlDocument> Parsing(String website)
        {
            HtmlDocument document = new HtmlDocument();
            using (HttpClient client = new HttpClient())
            using (HttpResponseMessage response = await client.GetAsync(website))
            using (HttpContent content = response.Content)
            {
                string result = await content.ReadAsStringAsync();
                document.LoadHtml(result);
            }
            return document;
        }

        public static void RemoveComments(HtmlNode node)
        {
            foreach (var n in node.ChildNodes.ToArray())
                RemoveComments(n);
            if (node.NodeType == HtmlNodeType.Comment)
                node.Remove();
        }


        public static async Task<String> ParsingDetik(String website, String type = "")
        {
            var node = await Parsing(website);

            var root = node.DocumentNode;

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


        public static async Task<String> ParsingTempo(String website, String type = "")
        {
            var node = await Parsing(website);

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
