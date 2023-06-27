using Microsoft.AspNetCore.Mvc;

namespace MyConversation.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
