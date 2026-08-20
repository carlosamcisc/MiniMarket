namespace MiniMarket.Models.ViewModels
{
    public class ReporteViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public int TotalVentas { get; set; }
        public decimal TotalIngresos { get; set; }
        public decimal TicketPromedio { get; set; }

        public List<VentaPorDia> VentasPorDia { get; set; } = new();
        public List<ProductoVendido> TopProductos { get; set; } = new();
        public List<VentaPorVendedor> VentasPorVendedor { get; set; } = new();

        public int UmbralStockBajo { get; set; } = 5;
        public List<ProductoStockBajo> ProductosStockBajo { get; set; } = new();

        public List<Movimiento> MovimientosRecientes { get; set; } = new();
        public int TotalEntradas { get; set; }
        public int TotalSalidas { get; set; }
    }

    public class VentaPorDia
    {
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }

    public class ProductoVendido
    {
        public string Nombre { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalVendido { get; set; }
    }

    public class VentaPorVendedor
    {
        public string Nombre { get; set; } = string.Empty;
        public int NumeroVentas { get; set; }
        public decimal TotalVendido { get; set; }
    }

    public class ProductoStockBajo
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Codigo { get; set; }
        public int Stock { get; set; }
    }
}
