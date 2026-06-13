using Microsoft.AspNetCore.Mvc;

namespace MiniMarket.Controllers
{
    public class AutenticacionController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        // Acción para cerrar sesión
        public async Task<IActionResult> Logout()
        {
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirige al login o inicio
            return RedirectToAction("Login", "Autenticacion");
        }
    }
}
