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
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        public async Task<IActionResult> Search(string searchAlgorithm, string queryText)
        {
            // Return these values to the view so that the view would not be reset after every search.
            ViewData["searchAlgorithm"] = searchAlgorithm;
            ViewData["queryText"] = queryText;

            // DEBUG: Make debugging faster
            // WARNING!!!: remove limit at run time
            int postCountLimit = 5;

            var posts = new List<Post>(await RssParser.ReadFeedsAsync(postCountLimit));
            await HtmlParser.FetchPostContents(posts);
            SearchAlgorithm.FilterPosts(posts, queryText, searchAlgorithm);
            return View(posts);
        }
    }
}
