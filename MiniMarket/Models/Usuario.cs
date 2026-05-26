using System;
using System.Collections.Generic;

namespace MiniMarket.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? RolId { get; set; }

    public virtual Role? Rol { get; set; }

    public virtual ICollection<Venta> Venta { get; set; } = new List<Venta>();
}
