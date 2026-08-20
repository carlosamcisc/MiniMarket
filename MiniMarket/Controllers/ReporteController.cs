using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniMarket.Models;
using MiniMarket.Models.ViewModels;

namespace MiniMarket.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ReporteController : Controller
    {
        private const int UmbralStockBajo = 5;

        private readonly MiniMarketContext _context;

        public ReporteController(MiniMarketContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fechaInicio, DateTime? fechaFin, bool todo = false)
        {
            var vm = await ConstruirReporte(fechaInicio, fechaFin, todo);
            return View(vm);
        }

        public async Task<IActionResult> ExportarPdf(DateTime? fechaInicio, DateTime? fechaFin, bool todo = false)
        {
            var vm = await ConstruirReporte(fechaInicio, fechaFin, todo);

            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            var normal = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            var bold = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            document.Add(new Paragraph("MiniMarket - Reporte de estadisticas")
                .SetFont(bold)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Periodo: {vm.FechaInicio:dd/MM/yyyy} - {vm.FechaFin:dd/MM/yyyy}")
                .SetFont(normal)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph(" "));

            // KPIs
            document.Add(new Paragraph("Resumen de ventas").SetFont(bold).SetFontSize(13));
            var kpiTable = new Table(3).UseAllAvailableWidth();
            kpiTable.AddHeaderCell(new Cell().Add(new Paragraph("Total de ventas").SetFont(bold).SetFontSize(9)));
            kpiTable.AddHeaderCell(new Cell().Add(new Paragraph("Ingresos totales").SetFont(bold).SetFontSize(9)));
            kpiTable.AddHeaderCell(new Cell().Add(new Paragraph("Ticket promedio").SetFont(bold).SetFontSize(9)));
            kpiTable.AddCell(new Cell().Add(new Paragraph(vm.TotalVentas.ToString()).SetFont(normal).SetFontSize(9)));
            kpiTable.AddCell(new Cell().Add(new Paragraph($"${vm.TotalIngresos:0.00}").SetFont(normal).SetFontSize(9)));
            kpiTable.AddCell(new Cell().Add(new Paragraph($"${vm.TicketPromedio:0.00}").SetFont(normal).SetFontSize(9)));
            document.Add(kpiTable);

            document.Add(new Paragraph(" "));

            // Top productos
            document.Add(new Paragraph("Productos mas vendidos").SetFont(bold).SetFontSize(13));
            var topTable = new Table(3).UseAllAvailableWidth();
            topTable.AddHeaderCell(new Cell().Add(new Paragraph("Producto").SetFont(bold).SetFontSize(9)));
            topTable.AddHeaderCell(new Cell().Add(new Paragraph("Cantidad vendida").SetFont(bold).SetFontSize(9)));
            topTable.AddHeaderCell(new Cell().Add(new Paragraph("Total vendido").SetFont(bold).SetFontSize(9)));
            foreach (var p in vm.TopProductos)
            {
                topTable.AddCell(new Cell().Add(new Paragraph(p.Nombre).SetFont(normal).SetFontSize(9)));
                topTable.AddCell(new Cell().Add(new Paragraph(p.CantidadVendida.ToString()).SetFont(normal).SetFontSize(9)));
                topTable.AddCell(new Cell().Add(new Paragraph($"${p.TotalVendido:0.00}").SetFont(normal).SetFontSize(9)));
            }
            if (!vm.TopProductos.Any())
            {
                topTable.AddCell(new Cell(1, 3).Add(new Paragraph("Sin ventas en el periodo").SetFont(normal).SetFontSize(9)));
            }
            document.Add(topTable);

            document.Add(new Paragraph(" "));

            // Ventas por vendedor
            document.Add(new Paragraph("Desempeno por vendedor").SetFont(bold).SetFontSize(13));
            var vendTable = new Table(3).UseAllAvailableWidth();
            vendTable.AddHeaderCell(new Cell().Add(new Paragraph("Vendedor").SetFont(bold).SetFontSize(9)));
            vendTable.AddHeaderCell(new Cell().Add(new Paragraph("Numero de ventas").SetFont(bold).SetFontSize(9)));
            vendTable.AddHeaderCell(new Cell().Add(new Paragraph("Total vendido").SetFont(bold).SetFontSize(9)));
            foreach (var v in vm.VentasPorVendedor)
            {
                vendTable.AddCell(new Cell().Add(new Paragraph(v.Nombre).SetFont(normal).SetFontSize(9)));
                vendTable.AddCell(new Cell().Add(new Paragraph(v.NumeroVentas.ToString()).SetFont(normal).SetFontSize(9)));
                vendTable.AddCell(new Cell().Add(new Paragraph($"${v.TotalVendido:0.00}").SetFont(normal).SetFontSize(9)));
            }
            if (!vm.VentasPorVendedor.Any())
            {
                vendTable.AddCell(new Cell(1, 3).Add(new Paragraph("Sin ventas en el periodo").SetFont(normal).SetFontSize(9)));
            }
            document.Add(vendTable);

            document.Add(new Paragraph(" "));

            // Stock bajo
            document.Add(new Paragraph($"Productos con stock bajo (<= {vm.UmbralStockBajo})").SetFont(bold).SetFontSize(13));
            var stockTable = new Table(3).UseAllAvailableWidth();
            stockTable.AddHeaderCell(new Cell().Add(new Paragraph("Producto").SetFont(bold).SetFontSize(9)));
            stockTable.AddHeaderCell(new Cell().Add(new Paragraph("Codigo").SetFont(bold).SetFontSize(9)));
            stockTable.AddHeaderCell(new Cell().Add(new Paragraph("Stock").SetFont(bold).SetFontSize(9)));
            foreach (var p in vm.ProductosStockBajo)
            {
                stockTable.AddCell(new Cell().Add(new Paragraph(p.Nombre).SetFont(normal).SetFontSize(9)));
                stockTable.AddCell(new Cell().Add(new Paragraph(p.Codigo ?? "-").SetFont(normal).SetFontSize(9)));
                stockTable.AddCell(new Cell().Add(new Paragraph(p.Stock.ToString()).SetFont(normal).SetFontSize(9)));
            }
            if (!vm.ProductosStockBajo.Any())
            {
                stockTable.AddCell(new Cell(1, 3).Add(new Paragraph("No hay productos con stock bajo").SetFont(normal).SetFontSize(9)));
            }
            document.Add(stockTable);

            document.Close();

            var nombreArchivo = $"reporte_{vm.FechaInicio:yyyyMMdd}_{vm.FechaFin:yyyyMMdd}.pdf";
            return File(ms.ToArray(), "application/pdf", nombreArchivo);
        }

        private async Task<ReporteViewModel> ConstruirReporte(DateTime? fechaInicio, DateTime? fechaFin, bool todo = false)
        {
            var hoy = DateTime.Today;
            DateTime inicio;
            DateTime fin;

            if (todo)
            {
                var primeraVenta = await _context.Ventas.Where(v => v.Fecha != null).MinAsync(v => (DateTime?)v.Fecha);
                inicio = (primeraVenta ?? hoy).Date;
                fin = hoy;
            }
            else
            {
                inicio = (fechaInicio ?? new DateTime(hoy.Year, hoy.Month, 1)).Date;
                fin = (fechaFin ?? hoy).Date;
            }

            if (fin < inicio)
            {
                (inicio, fin) = (fin, inicio);
            }

            var finExclusivo = fin.AddDays(1);

            var ventasEnRango = _context.Ventas
                .Include(v => v.DetalleVenta)
                .Include(v => v.Usuario)
                .Where(v => v.Fecha >= inicio && v.Fecha < finExclusivo);

            var ventas = await ventasEnRango.ToListAsync();

            var vm = new ReporteViewModel
            {
                FechaInicio = inicio,
                FechaFin = fin,
                UmbralStockBajo = UmbralStockBajo,
                TotalVentas = ventas.Count,
                TotalIngresos = ventas.Sum(v => v.Total ?? 0),
            };

            vm.TicketPromedio = vm.TotalVentas > 0 ? vm.TotalIngresos / vm.TotalVentas : 0;

            vm.VentasPorDia = ventas
                .Where(v => v.Fecha.HasValue)
                .GroupBy(v => v.Fecha!.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new VentaPorDia { Fecha = g.Key, Total = g.Sum(v => v.Total ?? 0) })
                .ToList();

            vm.TopProductos = ventas
                .SelectMany(v => v.DetalleVenta)
                .Where(d => d.Producto != null)
                .GroupBy(d => d.Producto!.Nombre)
                .Select(g => new ProductoVendido
                {
                    Nombre = g.Key,
                    CantidadVendida = g.Sum(d => d.Cantidad ?? 0),
                    TotalVendido = g.Sum(d => (d.Precio ?? 0) * (d.Cantidad ?? 0))
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(10)
                .ToList();

            vm.VentasPorVendedor = ventas
                .GroupBy(v => v.Usuario != null ? v.Usuario.Nombre : "Sin asignar")
                .Select(g => new VentaPorVendedor
                {
                    Nombre = g.Key,
                    NumeroVentas = g.Count(),
                    TotalVendido = g.Sum(v => v.Total ?? 0)
                })
                .OrderByDescending(v => v.TotalVendido)
                .ToList();

            vm.ProductosStockBajo = await _context.Inventarios
                .Include(i => i.Producto)
                .Where(i => i.Producto != null && (i.Stock ?? 0) <= UmbralStockBajo)
                .OrderBy(i => i.Stock)
                .Select(i => new ProductoStockBajo
                {
                    Nombre = i.Producto!.Nombre,
                    Codigo = i.Producto.Codigo,
                    Stock = i.Stock ?? 0
                })
                .ToListAsync();

            var movimientosEnRango = await _context.Movimientos
                .Include(m => m.Producto)
                .Where(m => m.Fecha >= inicio && m.Fecha < finExclusivo)
                .OrderByDescending(m => m.Fecha)
                .ToListAsync();

            vm.MovimientosRecientes = movimientosEnRango.Take(50).ToList();
            vm.TotalEntradas = movimientosEnRango
                .Where(m => string.Equals(m.Tipo, "entrada", StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Cantidad ?? 0);
            vm.TotalSalidas = movimientosEnRango
                .Where(m => string.Equals(m.Tipo, "salida", StringComparison.OrdinalIgnoreCase))
                .Sum(m => m.Cantidad ?? 0);

            return vm;
        }
    }
}
