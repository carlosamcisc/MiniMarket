using System;
using System.Collections.Generic;

namespace MiniMarket.Models;

public partial class Inventario
{
    public int Id { get; set; }

    public int? ProductoId { get; set; }

    public int? Stock { get; set; }

    public virtual Producto? Producto { get; set; }
}
