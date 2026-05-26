using System;
using System.Collections.Generic;

namespace MiniMarket.Models;

public partial class Venta
{
    public int Id { get; set; }

    public DateTime? Fecha { get; set; }

    public int? UsuarioId { get; set; }

    public decimal? Total { get; set; }

    public virtual ICollection<DetalleVentum> DetalleVenta { get; set; } = new List<DetalleVentum>();

    public virtual Usuario? Usuario { get; set; }
}
