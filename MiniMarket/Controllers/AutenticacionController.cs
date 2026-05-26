using Microsoft.AspNetCore.Mvc;

namespace MiniMarket.Controllers
{
    public class AutenticacionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
