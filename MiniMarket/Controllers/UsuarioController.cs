using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMarket.Models;
using MiniMarket.Services;

namespace MiniMarket.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuarioController : Controller
    {
        private readonly MiniMarketContext _context;

        public UsuarioController(MiniMarketContext context)
        {
            _context = context;
        }

        private async Task<List<Role>> RolesAsignables()
        {
            return await _context.Roles
                .Where(r => r.Nombre != "Administrador")
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        // GET: Usuario/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new SelectList(await RolesAsignables(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string nombre, string correo, string password, int rolId)
        {
            var roles = await RolesAsignables();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(string.Empty, "Completa todos los campos.");
            }
            else if (!roles.Any(r => r.Id == rolId))
            {
                ModelState.AddModelError(string.Empty, "Selecciona un rol válido.");
            }
            else if (await _context.Usuarios.AnyAsync(u => u.Correo == correo))
            {
                ModelState.AddModelError(string.Empty, "Ya existe un usuario con ese correo.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(roles, "Id", "Nombre", rolId);
                return View();
            }

            var usuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Password = PasswordHasher.Hash(password),
                RolId = rolId
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Usuario \"{nombre}\" creado correctamente.";
            return RedirectToAction(nameof(Create));
        }
    }
}
