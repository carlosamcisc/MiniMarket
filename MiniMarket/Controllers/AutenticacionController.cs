using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarket.Models;
using MiniMarket.Services;

namespace MiniMarket.Controllers
{
    [AllowAnonymous]
    public class AutenticacionController : Controller
    {
        private readonly MiniMarketContext _context;

        public AutenticacionController(MiniMarketContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string password)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                return Json(new { success = false, message = "Por favor, completa todos los campos" });
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null || !PasswordHasher.Verify(password, usuario.Password))
            {
                return Json(new { success = false, message = "Usuario o contraseña incorrectos" });
            }

            var rol = usuario.Rol?.Nombre ?? "";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            var (controller, action) = rol.ToLowerInvariant() switch
            {
                "vendedor" => ("Venta", "create"),
                "gerente" => ("Producto", "Index"),
                _ => ("Home", "Index")
            };

            return Json(new { success = true, controller, action });
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Autenticacion");
        }
    }
}
