using System;
using System.Collections.Generic;

namespace MiniMarket.Models;

public partial class Movimiento
{
    public int Id { get; set; }

    public int? ProductoId { get; set; }

    public string? Tipo { get; set; }

    public int? Cantidad { get; set; }

    public DateTime? Fecha { get; set; }

    public virtual Producto? Producto { get; set; }
}
