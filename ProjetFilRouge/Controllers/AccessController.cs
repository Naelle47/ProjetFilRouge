using Microsoft.AspNetCore.Mvc;

namespace ProjetFilRouge.Controllers
{
    public class AccessController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
