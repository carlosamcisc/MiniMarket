using System;
using System.Collections.Generic;

namespace MiniMarket.Models;

public partial class Producto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Codigo { get; set; }

    public decimal Precio { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual ICollection<Inventario> Inventarios { get; set; } = new List<Inventario>();

    public virtual ICollection<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

}
