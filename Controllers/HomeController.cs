using Microsoft.AspNetCore.Mvc;
using news_search.Libs;
using System.Threading.Tasks;
using System.Collections.Generic;
using news_search.Models;

namespace news_search.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> Search(string searchAlgorithm, string queryText)
        {
            var posts = new List<Post>(await RssParser.ReadFeedsAsync());
            await HtmlParser.FetchPostContents(posts);
            SearchAlgorithm.FilterPosts(posts, queryText, searchAlgorithm);
            return View(posts);
        }
    }
}
