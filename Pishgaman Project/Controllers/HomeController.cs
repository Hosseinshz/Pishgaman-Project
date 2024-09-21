using Microsoft.AspNetCore.Mvc;

namespace Pishgaman_Project.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }       
    }
}
