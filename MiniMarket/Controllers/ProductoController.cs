using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Build.Tasks.Deployment.Bootstrapper;
using Microsoft.EntityFrameworkCore;
using MiniMarket.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniMarket.Controllers
{
    public class ProductoController : Controller
    {
        private readonly MiniMarketContext _context;

        public ProductoController(MiniMarketContext context)
        {
            _context = context;
        }


        // GET: Producto
        public async Task<IActionResult> Index()
        {
            var productos = _context.Productos
                .Include(p => p.Inventarios)
                .ToListAsync();
            return View( await productos);
            //return View(await _context.Productos.ToListAsync());
        }



        //buscar producto por codigo o nombre
        public async Task<IActionResult> Buscar(string buscar)
        {
            ViewBag.Busqueda = buscar;
            var productos = from p in _context.Productos
                            select p;

            if (!string.IsNullOrEmpty(buscar))
            {
                productos = productos.Where(p =>
                    p.Nombre.Contains(buscar) ||
                    p.Codigo.Contains(buscar));
            }

            return View("Index", await productos.ToListAsync());
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            // traer inventario
            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.ProductoId == producto.Id);

            ViewBag.Stock = inventario?.Stock ?? 0;

            return View(producto);
        }

        // GET: Producto/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nombre,Codigo,Precio")] Producto producto, int stock)
        {
            if (ModelState.IsValid)
            {
                _context.Add(producto);
                await _context.SaveChangesAsync();
                //guardar el producto en el inventario
                var inventario = new Inventario
                {
                    ProductoId = producto.Id,
                    Stock = stock
                };
                _context.Inventarios.Add(inventario);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }

        // GET: Producto/Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }
            // traer inventario
            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.ProductoId == producto.Id);

            ViewBag.Stock = inventario?.Stock ?? 0;
            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Codigo,Precio")] Producto producto, int stock)
        {
            if (id != producto.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // actualizar producto
                    _context.Update(producto);
                    await _context.SaveChangesAsync();

                    // actualizar inventario
                    var inventario = await _context.Inventarios
                        .FirstOrDefaultAsync(i => i.ProductoId == producto.Id);

                    if (inventario != null)
                    {
                        inventario.Stock = stock;
                        _context.Update(inventario);
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductoExists(producto.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(producto);
        }


        // GET: Producto/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var producto = await _context.Productos
                .FirstOrDefaultAsync(m => m.Id == id);
            if (producto == null)
            {
                return NotFound();
            }
            // traer inventario
            var inventario = await _context.Inventarios
                .FirstOrDefaultAsync(i => i.ProductoId == producto.Id);

            ViewBag.Stock = inventario?.Stock ?? 0;
            return View(producto);
        }

        // POST: Producto/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
           
            var producto = await _context.Productos.FindAsync(id);

            if (producto != null)
            {
                // eliminar inventario relacionado primero
                var inventarios = _context.Inventarios
                    .Where(i => i.ProductoId == id);

                _context.Inventarios.RemoveRange(inventarios);

                // (opcional pero recomendado) eliminar detalles de venta
                var detalles = _context.DetalleVenta
                    .Where(d => d.ProductoId == id);

                _context.DetalleVenta.RemoveRange(detalles);

                // ahora sí eliminar producto
                _context.Productos.Remove(producto);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }
    }
}
