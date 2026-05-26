using System;
using System.Collections.Generic;

namespace MiniMarket.Models;

public partial class DetalleVentum
{
    public int Id { get; set; }

    public int? VentaId { get; set; }

    public int? ProductoId { get; set; }

    public int? Cantidad { get; set; }

    public decimal? Precio { get; set; }

    public virtual Producto? Producto { get; set; }

    public virtual Venta? Venta { get; set; }
}
