using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarket.Models;
using MiniMarket.Models.ViewModels;
using Newtonsoft.Json;
using System.IO;

namespace MiniMarket.Controllers
{
    public class VentaController : Controller
    {
        private readonly MiniMarketContext _context;

        public VentaController(MiniMarketContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }


        //public IActionResult create()
        //{
        //    var vm = new ventaViewModel
        //    {
        //        productos = _context.Productos.ToList()
        //    };

        //    return View(vm);
        //}
        public IActionResult create()
        {
            var vm = new ventaViewModel
            {
                productos = _context.Productos
                    .Include(p => p.Inventarios) // ESTO FALTABA
                    .ToList()
            };

            return View(vm);
        }


        [HttpPost]
        public async Task<IActionResult> create(string detalleJson)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var detalles = JsonConvert.DeserializeObject<List<DetalleVentum>>(detalleJson);

                if (detalles == null || !detalles.Any())
                {
                    return BadRequest("no hay productos en la venta");
                }

                var venta = new Venta
                {
                    Fecha = DateTime.Now,
                    Total = detalles.Sum(d => d.Precio * d.Cantidad)
                };

                _context.Ventas.Add(venta);
                await _context.SaveChangesAsync();

                foreach (var d in detalles)
                {
                    var inv = _context.Inventarios
                        .FirstOrDefault(i => i.ProductoId == d.ProductoId);

                    if (inv == null)
                        throw new Exception($"no existe inventario para producto {d.ProductoId}");

                    if (inv.Stock < d.Cantidad)
                        throw new Exception($"stock insuficiente para producto {d.ProductoId}");

                    d.VentaId = venta.Id;
                    _context.DetalleVenta.Add(d);

                   
                    inv.Stock -= d.Cantidad;
                }

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return RedirectToAction("Confirmacion", new { id = venta.Id });

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }
        
        public IActionResult Confirmacion(int id)
        {
            ViewBag.Id = id;
            return View();
        }
        public async Task<IActionResult> Ticket(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);

            if (venta == null)
            {
                return NotFound("venta no encontrada");
            }

            var detalles = _context.DetalleVenta
                .Where(d => d.VentaId == id)
                .ToList();

            var productos = _context.Productos
                .ToDictionary(p => p.Id, p => p.Nombre);

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);

                var pageSize = new PageSize(226, 700);
                var document = new Document(pdf, pageSize);
                document.SetMargins(10, 10, 10, 10);

                //fuentes
                var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
                var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                // 🏪 título (grande y bold)
                document.Add(new Paragraph("mini market")
                    .SetFont(bold)
                    .SetFontSize(12)
                    .SetTextAlignment(TextAlignment.CENTER));

                // subtítulo
                document.Add(new Paragraph("gracias por tu compra")
                    .SetFont(normal)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Add(new Paragraph("----------------------------")
                    .SetFontSize(8));

                // info venta
                document.Add(new Paragraph($"fecha: {venta.Fecha}")
                    .SetFont(normal)
                    .SetFontSize(8));

                document.Add(new Paragraph($"folio: {venta.Id}")
                    .SetFont(normal)
                    .SetFontSize(8));

                document.Add(new Paragraph("----------------------------")
                    .SetFontSize(8));

                // 📦 productos
                foreach (var d in detalles)
                {
                    var nombre = productos.ContainsKey(d.ProductoId.Value)
                        ? productos[d.ProductoId.Value]
                        : "n/a";

                    // nombre producto (ligeramente más visible)
                    document.Add(new Paragraph(nombre)
                        .SetFont(bold)
                        .SetFontSize(9));

                    // detalle línea
                    document.Add(new Paragraph(
                        $"{d.Cantidad} x {d.Precio:0.00} = {(d.Cantidad * d.Precio):0.00}")
                        .SetFont(normal)
                        .SetFontSize(8));
                }

                document.Add(new Paragraph("----------------------------")
                    .SetFontSize(8));

                // 💰 total (grande y bold)
                document.Add(new Paragraph($"total: ${venta.Total:0.00}")
                    .SetFont(bold)
                    .SetFontSize(11)
                    .SetTextAlignment(TextAlignment.RIGHT));

                document.Add(new Paragraph("----------------------------")
                    .SetFontSize(8));

                // mensaje final
                document.Add(new Paragraph("vuelva pronto :)")
                    .SetFont(normal)
                    .SetFontSize(8)
                    .SetTextAlignment(TextAlignment.CENTER));

                document.Close();

                return File(ms.ToArray(), "application/pdf", $"ticket_{venta.Id}.pdf");
            }
        }


    }

}
