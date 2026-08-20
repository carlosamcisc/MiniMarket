using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MiniMarket.Models;
using MiniMarket.Services;

namespace MiniMarket.Controllers
{
    [Authorize]
    public class UsuarioController : Controller
    {
        private readonly MiniMarketContext _context;

        public UsuarioController(MiniMarketContext context)
        {
            _context = context;
        }

        private int? UsuarioActualId
        {
            get
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                return int.TryParse(idClaim, out var id) ? id : null;
            }
        }

        private async Task<List<Role>> RolesAsignables()
        {
            return await _context.Roles
                .Where(r => r.Nombre != "Administrador")
                .OrderBy(r => r.Nombre)
                .ToListAsync();
        }

        // GET: Usuario/Perfil
        public async Task<IActionResult> Perfil()
        {
            var id = UsuarioActualId;
            if (id == null)
            {
                return Forbid();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Venta)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuario
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            return View(usuarios);
        }

        // GET: Usuario/Details/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Details(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Venta)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuario/Edit/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            await _context.Entry(usuario).Reference(u => u.Rol).LoadAsync();

            if (usuario.Rol?.Nombre == "Administrador")
            {
                TempData["Error"] = "No se puede editar una cuenta de Administrador desde aquí.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Roles = new SelectList(await RolesAsignables(), "Id", "Nombre", usuario.RolId);
            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Edit(int id, string nombre, string correo, string? password, int rolId)
        {
            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.Rol?.Nombre == "Administrador")
            {
                TempData["Error"] = "No se puede editar una cuenta de Administrador desde aquí.";
                return RedirectToAction(nameof(Index));
            }

            var roles = await RolesAsignables();

            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(correo))
            {
                ModelState.AddModelError(string.Empty, "Completa todos los campos.");
            }
            else if (!roles.Any(r => r.Id == rolId))
            {
                ModelState.AddModelError(string.Empty, "Selecciona un rol válido.");
            }
            else if (await _context.Usuarios.AnyAsync(u => u.Correo == correo && u.Id != id))
            {
                ModelState.AddModelError(string.Empty, "Ya existe un usuario con ese correo.");
            }
            else if (!string.IsNullOrEmpty(password) && password.Length < 6)
            {
                ModelState.AddModelError(string.Empty, "La nueva contraseña debe tener al menos 6 caracteres.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = new SelectList(roles, "Id", "Nombre", rolId);
                usuario.Nombre = nombre;
                usuario.Correo = correo;
                usuario.RolId = rolId;
                return View(usuario);
            }

            usuario.Nombre = nombre;
            usuario.Correo = correo;
            usuario.RolId = rolId;

            if (!string.IsNullOrEmpty(password))
            {
                usuario.Password = PasswordHasher.Hash(password);
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Usuario \"{nombre}\" actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Usuario/Delete/5
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.Rol?.Nombre == "Administrador")
            {
                TempData["Error"] = "No se puede eliminar una cuenta de Administrador.";
                return RedirectToAction(nameof(Index));
            }

            if (usuario.Id == UsuarioActualId)
            {
                TempData["Error"] = "No puedes eliminar tu propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            return View(usuario);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
            {
                return NotFound();
            }

            if (usuario.Rol?.Nombre == "Administrador")
            {
                TempData["Error"] = "No se puede eliminar una cuenta de Administrador.";
                return RedirectToAction(nameof(Index));
            }

            if (usuario.Id == UsuarioActualId)
            {
                TempData["Error"] = "No puedes eliminar tu propia cuenta.";
                return RedirectToAction(nameof(Index));
            }

            _context.Usuarios.Remove(usuario);

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Usuario \"{usuario.Nombre}\" eliminado correctamente.";
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = $"No se puede eliminar \"{usuario.Nombre}\" porque tiene ventas registradas asociadas.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuario/Create
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Create()
        {
            ViewBag.Roles = new SelectList(await RolesAsignables(), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
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
