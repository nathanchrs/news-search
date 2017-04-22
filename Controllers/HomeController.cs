using Microsoft.AspNetCore.Mvc;
using news_search.Libs;
using news_search.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            IEnumerable<Post> postSearch = await SearchAlgorithm.GetAllPost(searchAlgorithm, queryText);
            return View(postSearch);
        }
    }
}
