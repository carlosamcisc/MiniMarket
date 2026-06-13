using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MiniMarket.Models;
using MiniMarket.Models.ViewModels;

namespace MiniMarket.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly MiniMarketContext _context;

        public HomeController(ILogger<HomeController> logger, MiniMarketContext context )
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            HomeViewModel vm = new HomeViewModel();
            ViewBag.TotalProductos = _context.Productos.Count();
            ViewBag.TotalVentas = _context.Ventas.Count();
            ViewBag.TotalIngresos = _context.DetalleVenta.Sum(d => d.Precio);
            ViewBag.UltimosProductos = _context.Productos
                .OrderByDescending(p => p.Id)
                .Take(4)
                .ToList();
            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
