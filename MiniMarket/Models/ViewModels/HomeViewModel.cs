namespace MiniMarket.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Producto> Producto { get; set; } = new();
        public List<Venta> Venta { get; set; } = new();
        public List<DetalleVentum> DetalleVenta { get; set; } = new();
        public int TotalProductos { get; set; }
        public int TotalVenta { get;set; }
        public int TotalIngresos { get; set; }
    }
}
